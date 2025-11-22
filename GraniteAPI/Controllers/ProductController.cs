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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    Price = p.Price,
                    ImageFileName = p.ImageFileName,
                    CategoryId = p.CategoryId,
                    Category = p.Category.Name
                }).ToListAsync();

            return Ok(products);
        }

        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto request)
        {
            string fileName = null; // <-- declare here

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                string base64Data = request.ImageBase64;

                // Remove "data:image/...;base64," if present
                if (base64Data.Contains(","))
                    base64Data = base64Data.Split(',')[1];

                var bytes = Convert.FromBase64String(base64Data);
                fileName = $"{Guid.NewGuid()}.png"; // <-- do NOT use 'var' here
                var path = Path.Combine(_imageFolder, fileName);
                await System.IO.File.WriteAllBytesAsync(path, bytes);
            }

            var product = new Product
            {
                Name = request.Name,
                Color = request.Color,
                Price = request.Price,
                CategoryId = request.CategoryId,
                ImageFileName = fileName
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Color = product.Color,
                Price = product.Price,
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
            product.Color = request.Color;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;
            product.ImageFileName = fileName;

            await _context.SaveChangesAsync();

            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Color = product.Color,
                Price = product.Price,
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

        [HttpGet("images/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            var path = Path.Combine(_imageFolder, fileName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            // Determine MIME type (optional)
            var ext = Path.GetExtension(fileName).ToLower();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, contentType);
        }

    }


}
