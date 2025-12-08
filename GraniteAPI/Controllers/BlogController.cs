using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _imageFolder;

        public BlogController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;

            var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _imageFolder = Path.Combine(webRoot, "images");

            if (!Directory.Exists(_imageFolder))
                Directory.CreateDirectory(_imageFolder);
        }

        // GET All
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var blogs = await _context.Blogs
                    .Select(b => new BlogDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Description = b.Description,
                        Content = b.Content,
                        ImageFileName = b.ImageFileName
                    }).ToListAsync();

                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch blogs", error = ex.Message });
            }
        }

        // POST Create
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] BlogCreateUpdateDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body" });

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Title is required" });

            string fileName = null;

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                try
                {
                    string base64 = request.ImageBase64;

                    if (base64.Contains(","))
                        base64 = base64.Split(',')[1];

                    var bytes = Convert.FromBase64String(base64);

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
                var blog = new Blog
                {
                    Title = request.Title,
                    Description = request.Description,
                    Content = request.Content,
                    ImageFileName = fileName
                };

                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();

                return Ok(new BlogDto
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    Description = blog.Description,
                    Content = blog.Content,
                    ImageFileName = blog.ImageFileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create blog", error = ex.Message });
            }
        }

        // PUT Update
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BlogCreateUpdateDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body" });

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound(new { message = "Blog not found with the provided ID" });

            string fileName = blog.ImageFileName;
            string oldFile = blog.ImageFileName;

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                try
                {
                    string base64 = request.ImageBase64;
                    if (base64.Contains(","))
                        base64 = base64.Split(',')[1];

                    var bytes = Convert.FromBase64String(base64);

                    fileName = $"{Guid.NewGuid()}.png";
                    string path = Path.Combine(_imageFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(path, bytes);

                    if (!string.IsNullOrEmpty(oldFile) && oldFile != fileName)
                    {
                        var oldPath = Path.Combine(_imageFolder, oldFile);
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
                blog.Title = request.Title;
                blog.Description = request.Description;
                blog.Content = request.Content;
                blog.ImageFileName = fileName;

                await _context.SaveChangesAsync();

                return Ok(new BlogDto
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    Description = blog.Description,
                    Content = blog.Content,
                    ImageFileName = blog.ImageFileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update blog", error = ex.Message });
            }
        }

        // DELETE
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound(new { message = "Blog not found with the provided ID" });

            try
            {
                if (!string.IsNullOrEmpty(blog.ImageFileName))
                {
                    var path = Path.Combine(_imageFolder, blog.ImageFileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Blog deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete blog", error = ex.Message });
            }
        }
    }
}
