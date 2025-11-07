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
            // Fallback if WebRootPath is null
            var webRoot = env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            _imageFolder = Path.Combine(webRoot, "images");

            // Ensure folder exists
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
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    Price = p.Price,
                    ImageFileName = p.ImageFileName,
                    CategoryId = p.CategoryId,
                    Category = p.Category.Name
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromForm] ProductCreateUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // <-- this is likely triggering 400

            // Save file
            string fileName = null;
            if (request.Image != null)
            {
                fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                var filePath = Path.Combine(_imageFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await request.Image.CopyToAsync(stream);
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
        public async Task<IActionResult> Update(int id, [FromForm] ProductCreateUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Invalid CategoryId" });

            // Save file if provided
            if (request.Image != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                var filePath = Path.Combine(_imageFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await request.Image.CopyToAsync(stream);
                product.ImageFileName = fileName;
            }

            // Update other fields
            product.Name = request.Name;
            product.Color = request.Color;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;

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
            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Delete image file
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
