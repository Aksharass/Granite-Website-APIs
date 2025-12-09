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
        /// Get All Categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch categories", error = ex.Message });
            }
        }

        /// <summary>
        /// Get Category By ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch category", error = ex.Message });
            }
        }

        /// <summary>
        /// Create Category
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data", errors = ModelState });

            try
            {
                if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                    return Conflict(new { message = "Category name already exists" });

                var category = new Category { Name = dto.Name };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Get), new { id = category.Id }, new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create category", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Category
        /// </summary>
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data", errors = ModelState });

            try
            {
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update category", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Category
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound(new { message = "Category not found" });

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete category", error = ex.Message });
            }
        }

        
        [HttpGet("with-subcategories")]
        public async Task<IActionResult> GetAllWithSubCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.SubCategories)
                    .AsNoTracking()
                    .Select(c => new CategoryWithSubCategoriesDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        SubCategories = c.SubCategories.Select(sc => new SubCategoryDto
                        {
                            Id = sc.Id,
                            Name = sc.Name,
                            CategoryId = sc.CategoryId,
                            CategoryName = c.Name
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch categories", error = ex.Message });
            }
        }

        [HttpGet("with-subcategories/{id:int}")]
        public async Task<IActionResult> GetByIdWithSubCategories(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.SubCategories)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound(new { message = "Category not found" });

                var result = new CategoryWithSubCategoriesDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    SubCategories = category.SubCategories.Select(sc => new SubCategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        CategoryId = sc.CategoryId,
                        CategoryName = category.Name
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch category", error = ex.Message });
            }
        }

        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> GetCategoryTree(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc => sc.Products)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound(new { message = "Category not found" });

                var result = new
                {
                    id = category.Id,
                    name = category.Name,

                    subCategories = category.SubCategories.Select(sc => new
                    {
                        id = sc.Id,
                        name = sc.Name,

                        products = sc.Products.Select(p => new ProductDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Brand = p.Brand,
                            Size = p.Size,
                            ImageUrl = p.ImageUrl,
                            CategoryId = p.CategoryId,
                            Category = category.Name,
                            SubCategoryId = p.SubCategoryId,
                            SubCategoryName = sc.Name
                        }).ToList()
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch category tree", error = ex.Message });
            }
        }

    }
}
