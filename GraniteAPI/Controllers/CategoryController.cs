using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get All Categories (With Products List)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Get Category By ID (No products)
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            return Ok(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            });
        }

        /// <summary>
        /// Create a Category
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                return Conflict(new { message = "Category name already exists" });

            var category = new Category
            {
                Name = dto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = category.Id }, new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            });
        }

        /// <summary>
        /// Update Category
        /// </summary>
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dbCategory = await _context.Categories.FindAsync(id);
            if (dbCategory == null)
                return NotFound(new { message = "Category not found" });

            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
                return Conflict(new { message = "Another category with this name already exists" });

            dbCategory.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new CategoryDto
            {
                Id = dbCategory.Id,
                Name = dbCategory.Name
            });
        }

        /// <summary>
        /// Delete Category
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully" });
        }

        /// <summary>
        /// Get All Categories with Products Details
        /// </summary>
        [HttpGet("details")]
        public async Task<IActionResult> GetAllWithProducts()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .Select(c => new CategoryWithProductsDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Products = c.Products.Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Color = p.Color,
                        Price = p.Price,
                        ImageFileName = p.ImageFileName, // updated here
                        CategoryId = p.CategoryId,
                        Category = p.Category.Name
                    }).ToList()
                }).ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Get Category By Id With Product Details
        /// </summary>
        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> GetByIdWithProducts(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            var result = new CategoryWithProductsDto
            {
                Id = category.Id,
                Name = category.Name,
                Products = category.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    Price = p.Price,
                    ImageFileName = p.ImageFileName, // updated here
                    CategoryId = p.CategoryId,
                    Category = p.Category.Name
                }).ToList()
            };

            return Ok(result);
        }
    }
}
