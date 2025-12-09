using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // GET ALL PRODUCTS
        // ===============================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Brand = p.Brand,
                    Size = p.Size,
                    CategoryId = p.CategoryId,
                    Category = p.Category.Name,
                    SubCategoryId = p.SubCategoryId,
                    SubCategoryName = p.SubCategory != null ? p.SubCategory.Name : null,
                    ImageBase64 = p.ImageData != null
                        ? $"data:{p.ImageMimeType};base64,{Convert.ToBase64String(p.ImageData)}"
                        : null
                })
                .ToListAsync();

            return Ok(products);
        }

        // ===============================
        // INSERT PRODUCT
        // ===============================
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto request)
        {
            byte[]? imageBytes = null;
            string? mimeType = null;

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                string base64 = request.ImageBase64;

                if (base64.Contains(","))
                    base64 = base64.Split(',')[1];

                imageBytes = Convert.FromBase64String(base64);
                mimeType = "image/png";   // or detect dynamically
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Brand = request.Brand,
                Size = request.Size,
                CategoryId = request.CategoryId,
                SubCategoryId = request.SubCategoryId,
                ImageData = imageBytes,
                ImageMimeType = mimeType
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var category = await _context.Categories.FindAsync(product.CategoryId);
            var subCategory = product.SubCategoryId != null
                ? await _context.SubCategories.FindAsync(product.SubCategoryId.Value)
                : null;

            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                Size = product.Size,
                CategoryId = product.CategoryId,
                Category = category?.Name ?? "",
                SubCategoryId = product.SubCategoryId,
                SubCategoryName = subCategory?.Name,
                ImageBase64 = product.ImageData != null
                    ? $"data:{product.ImageMimeType};base64,{Convert.ToBase64String(product.ImageData)}"
                    : null
            });
        }

        // ===============================
        // UPDATE PRODUCT
        // ===============================
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Update image only if new image is uploaded
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                string base64 = request.ImageBase64;

                if (base64.Contains(","))
                    base64 = base64.Split(',')[1];

                product.ImageData = Convert.FromBase64String(base64);
                product.ImageMimeType = "image/png";
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Brand = request.Brand;
            product.Size = request.Size;
            product.CategoryId = request.CategoryId;
            product.SubCategoryId = request.SubCategoryId;

            await _context.SaveChangesAsync();

            var category = await _context.Categories.FindAsync(product.CategoryId);
            var subCategory = product.SubCategoryId != null
                ? await _context.SubCategories.FindAsync(product.SubCategoryId.Value)
                : null;

            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                Size = product.Size,
                CategoryId = product.CategoryId,
                Category = category.Name,
                SubCategoryId = product.SubCategoryId,
                SubCategoryName = subCategory?.Name,
                ImageBase64 = product.ImageData != null
                    ? $"data:{product.ImageMimeType};base64,{Convert.ToBase64String(product.ImageData)}"
                    : null
            });
        }

        // ===============================
        // DELETE PRODUCT
        // ===============================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }
    }
}
