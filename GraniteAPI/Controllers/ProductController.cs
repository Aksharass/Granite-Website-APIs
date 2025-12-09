using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Models;
using GraniteAPI.Services;
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
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto request, [FromServices] CloudinaryService cloudinary)
        {
            string? imageUrl = null;

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                imageUrl = await cloudinary.UploadBase64ImageAsync(request.ImageBase64);
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Brand = request.Brand,
                Size = request.Size,
                CategoryId = request.CategoryId,
                SubCategoryId = request.SubCategoryId,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var category = await _context.Categories.FindAsync(product.CategoryId);
            var subCategory = product.SubCategoryId != null
                ? await _context.SubCategories.FindAsync(product.SubCategoryId.Value)
                : null;

            return Ok(product);
        }

        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto request, [FromServices] CloudinaryService cloudinary)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                product.ImageUrl = await cloudinary.UploadBase64ImageAsync(request.ImageBase64);
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

            return Ok(product);
        }

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
