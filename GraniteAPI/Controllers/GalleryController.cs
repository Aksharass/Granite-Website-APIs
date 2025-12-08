using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GalleryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _imageFolder;

        public GalleryController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;

            var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _imageFolder = Path.Combine(webRoot, "images");

            if (!Directory.Exists(_imageFolder))
                Directory.CreateDirectory(_imageFolder);
        }


        // GET All Gallery Images
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var items = await _context.Galleries
                    .Select(g => new GalleryDto
                    {
                        Id = g.Id,
                        ImageFileName = g.ImageFileName,
                    })
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch gallery images", error = ex.Message });
            }
        }


        // POST Insert
        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] GalleryCreateUpdateDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body" });

            if (string.IsNullOrEmpty(request.ImageBase64))
                return BadRequest(new { message = "ImageBase64 is required" });

            string fileName = null;

            // Save image (same as ProductController)
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

            // Save to DB
            try
            {
                var gallery = new Gallery
                {
                    ImageFileName = fileName,
                };

                _context.Galleries.Add(gallery);
                await _context.SaveChangesAsync();

                return Ok(new GalleryDto
                {
                    Id = gallery.Id,
                    ImageFileName = gallery.ImageFileName,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create gallery item", error = ex.Message });
            }
        }


        // PUT Update
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GalleryCreateUpdateDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body" });

            var item = await _context.Galleries.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Gallery item not found" });

            string fileName = item.ImageFileName;
            string oldFile = item.ImageFileName;

            // Update image if provided
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

                    // Delete old image
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
                item.ImageFileName = fileName;

                await _context.SaveChangesAsync();

                return Ok(new GalleryDto
                {
                    Id = item.Id,
                    ImageFileName = item.ImageFileName,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update gallery item", error = ex.Message });
            }
        }


        // DELETE
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Galleries.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Gallery item not found" });

            try
            {
                if (!string.IsNullOrEmpty(item.ImageFileName))
                {
                    var path = Path.Combine(_imageFolder, item.ImageFileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _context.Galleries.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Gallery item deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete gallery item", error = ex.Message });
            }
        }
    }
}
