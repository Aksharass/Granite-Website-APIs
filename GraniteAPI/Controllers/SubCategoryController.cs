using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Migrations;
using GraniteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get All SubCategories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var subCategories = await _context.SubCategories
                    .Include(sc => sc.Category)
                    .AsNoTracking()
                    .Select(sc => new SubCategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        CategoryId = sc.CategoryId,
                        CategoryName = sc.Category.Name
                    })
                    .ToListAsync();

                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch subcategories", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SubCategory By ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var subCategory = await _context.SubCategories
                    .Include(sc => sc.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sc => sc.Id == id);

                if (subCategory == null)
                    return NotFound(new { message = "Subcategory not found" });

                return Ok(new SubCategoryDto
                {
                    Id = subCategory.Id,
                    Name = subCategory.Name,
                    CategoryId = subCategory.CategoryId,
                    CategoryName = subCategory.Category.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch subcategory", error = ex.Message });
            }
        }

        /// <summary>
        /// Create SubCategory
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] SubCategoryCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data", errors = ModelState });

            try
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                    return NotFound(new { message = "Category not found" });

                if (await _context.SubCategories.AnyAsync(sc => sc.Name == dto.Name && sc.CategoryId == dto.CategoryId))
                    return Conflict(new { message = "Subcategory already exists in this category" });

                var subCategory = new SubCategory
                {
                    Name = dto.Name,
                    CategoryId = dto.CategoryId
                };

                _context.SubCategories.Add(subCategory);
                await _context.SaveChangesAsync();

                var category = await _context.Categories.FindAsync(subCategory.CategoryId);

                return CreatedAtAction(nameof(Get), new { id = subCategory.Id }, new SubCategoryDto
                {
                    Id = subCategory.Id,
                    Name = subCategory.Name,
                    CategoryId = subCategory.CategoryId,
                    CategoryName = category?.Name ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create subcategory", error = ex.Message });
            }
        }

        /// <summary>
        /// Update SubCategory
        /// </summary>
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SubCategoryCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data", errors = ModelState });

            try
            {
                var dbSubCategory = await _context.SubCategories.FindAsync(id);
                if (dbSubCategory == null)
                    return NotFound(new { message = "Subcategory not found" });

                if (await _context.SubCategories.AnyAsync(sc => sc.Name == dto.Name && sc.CategoryId == dto.CategoryId && sc.Id != id))
                    return Conflict(new { message = "Another subcategory with the same name exists in this category" });

                dbSubCategory.Name = dto.Name;
                dbSubCategory.CategoryId = dto.CategoryId;

                await _context.SaveChangesAsync();
                var category = await _context.Categories.FindAsync(dbSubCategory.CategoryId);

                return Ok(new SubCategoryDto
                {
                    Id = dbSubCategory.Id,
                    Name = dbSubCategory.Name,
                    CategoryId = dbSubCategory.CategoryId,
                    CategoryName = category?.Name ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update subcategory", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete SubCategory
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var subCategory = await _context.SubCategories.FindAsync(id);
                if (subCategory == null)
                    return NotFound(new { message = "Subcategory not found" });

                _context.SubCategories.Remove(subCategory);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Subcategory deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete subcategory", error = ex.Message });
            }
        }

        /// <summary>
        /// Get All SubCategories with Products
        /// </summary>
        [HttpGet("details")]
        public async Task<IActionResult> GetAllWithProducts()
        {
            try
            {
                var subCategories = await _context.SubCategories
                    .Include(sc => sc.Products)
                    .Include(sc => sc.Category)
                    .AsNoTracking()
                    .Select(sc => new SubCategoryWithProductsDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        CategoryId = sc.CategoryId,
                        CategoryName = sc.Category.Name,
                        Products = sc.Products.Select(p => new ProductDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Brand = p.Brand,
                            Size = p.Size,
                            ImageUrl = p.ImageUrl,
                            CategoryId = p.CategoryId,
                            Category = p.Category.Name,
                            SubCategoryId = p.SubCategoryId,
                            SubCategoryName = sc.Name
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch subcategory details", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SubCategory By ID with Products
        /// </summary>
        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> GetByIdWithProducts(int id)
        {
            try
            {
                var subCategory = await _context.SubCategories
                    .Include(sc => sc.Products)
                    .Include(sc => sc.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sc => sc.Id == id);

                if (subCategory == null)
                    return NotFound(new { message = "Subcategory not found" });

                var result = new SubCategoryWithProductsDto
                {
                    Id = subCategory.Id,
                    Name = subCategory.Name,
                    CategoryId = subCategory.CategoryId,
                    CategoryName = subCategory.Category.Name,
                    Products = subCategory.Products.Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Brand = p.Brand,
                        Size = p.Size,
                        ImageUrl = p.ImageUrl,
                        CategoryId = p.CategoryId,
                        Category = p.Category.Name,
                        SubCategoryId = p.SubCategoryId,
                        SubCategoryName = subCategory.Name

                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch subcategory details", error = ex.Message });
            }
        }
    }
}
