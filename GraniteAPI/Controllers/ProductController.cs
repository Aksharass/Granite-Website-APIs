using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
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
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto request)
        {
            string fileName = null;

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                try
                {
                    string base64Data = request.ImageBase64;

                    // clean base64
                    if (base64Data.Contains(","))
                        base64Data = base64Data.Split(',')[1];

                    byte[] bytes = Convert.FromBase64String(base64Data);

                    // ensure folder exists
                    string imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imageFolder))
                        Directory.CreateDirectory(imageFolder);

                    fileName = $"{Guid.NewGuid()}.png";
                    string filePath = Path.Combine(imageFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = "Invalid Base64 image", error = ex.Message });
                }
            }

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
                ImageFileName = product.ImageFileName
            });
        }



        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Validate Category
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Invalid CategoryId" });

            string fileName = product.ImageFileName; // existing image
            string oldFileName = product.ImageFileName;

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                string base64Data = request.ImageBase64;

                if (base64Data.Contains(","))
                    base64Data = base64Data.Split(',')[1];

                var bytes = Convert.FromBase64String(base64Data);

                // Generate new file name
                fileName = $"{Guid.NewGuid()}.png";
                var path = Path.Combine(_imageFolder, fileName);

                await System.IO.File.WriteAllBytesAsync(path, bytes);

                // Delete old image if it's different
                if (!string.IsNullOrEmpty(oldFileName) && oldFileName != fileName)
                {
                    var oldPath = Path.Combine(_imageFolder, oldFileName);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
            }

            // Update fields
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
                ImageFileName = product.ImageFileName
            });
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { message = "Product not found" });

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

    }


}
