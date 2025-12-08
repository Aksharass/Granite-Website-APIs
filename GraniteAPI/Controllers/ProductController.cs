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
        private readonly string _imageFolder;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _imageFolder = Path.Combine(webRoot, "images");

            if (!Directory.Exists(_imageFolder))
                Directory.CreateDirectory(_imageFolder);
        }

        // GET All Products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Brand = p.Brand,
                        Size = p.Size,
                        ImageFileName = p.ImageFileName,
                        CategoryId = p.CategoryId,
                        Category = p.Category.Name
                    }).ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch products", error = ex.Message });
            }
        }


        // POST Create Product
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body" });

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Product name is required" });

            // Validate Category
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Invalid CategoryId" });

            string fileName = null;

            // Handle image
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                try
                {
                    string base64Data = request.ImageBase64;

                    if (base64Data.Contains(","))
                        base64Data = base64Data.Split(',')[1];

                    var bytes = Convert.FromBase64String(base64Data);

                    fileName = $"{Guid.NewGuid()}.png";
                    string filePath = Path.Combine(_imageFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                }
                catch
                {
                    return BadRequest(new { message = "Invalid Base64 image format" });
                }
            }

            try
            {
                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Brand = request.Brand,
                    Size = request.Size,
                    CategoryId = request.CategoryId,
                    ImageFileName = fileName
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Brand = product.Brand,
                    Size = product.Size,
                    CategoryId = product.CategoryId,
                    ImageFileName = product.ImageFileName,
                    Category = category.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create product", error = ex.Message });
            }
        }


        // PUT Update Product
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body" });

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Validate Category
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Invalid CategoryId" });

            string fileName = product.ImageFileName;
            string oldFileName = product.ImageFileName;

            // Handle image
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                try
                {
                    string base64Data = request.ImageBase64;

                    if (base64Data.Contains(","))
                        base64Data = base64Data.Split(',')[1];

                    var bytes = Convert.FromBase64String(base64Data);

                    fileName = $"{Guid.NewGuid()}.png";
                    string path = Path.Combine(_imageFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(path, bytes);

                    // Remove old image
                    if (!string.IsNullOrEmpty(oldFileName) && oldFileName != fileName)
                    {
                        var oldPath = Path.Combine(_imageFolder, oldFileName);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                }
                catch
                {
                    return BadRequest(new { message = "Invalid Base64 image format" });
                }
            }

            try
            {
                product.Name = request.Name;
                product.Description = request.Description;
                product.Brand = request.Brand;
                product.Size = request.Size;
                product.CategoryId = request.CategoryId;
                product.ImageFileName = fileName;

                await _context.SaveChangesAsync();

                return Ok(new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Brand = product.Brand,
                    Size = product.Size,
                    CategoryId = product.CategoryId,
                    ImageFileName = product.ImageFileName,
                    Category = category.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update product", error = ex.Message });
            }
        }


        // DELETE Product
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            try
            {
                if (!string.IsNullOrEmpty(product.ImageFileName))
                {
                    var path = Path.Combine(_imageFolder, product.ImageFileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete product", error = ex.Message });
            }
        }
    }
}
