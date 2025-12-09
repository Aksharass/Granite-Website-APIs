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
    public class GalleryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GalleryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: return ALL gallery images (no product filter)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var gallery = await _context.Galleries
                .Select(g => new GalleryDto
                {
                    Id = g.Id,
                    ImageBase64 = g.ImageData != null
                        ? $"data:{g.ImageMimeType};base64,{Convert.ToBase64String(g.ImageData)}"
                        : null
                })
                .ToListAsync();

            return Ok(gallery);
        }

        // INSERT gallery image (no productId)
        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] GalleryCreateDto request)
        {
            if (string.IsNullOrEmpty(request.ImageBase64))
                return BadRequest(new { message = "ImageBase64 is required" });

            string base64 = request.ImageBase64;
            if (base64.Contains(","))
                base64 = base64.Split(',')[1];

            var bytes = Convert.FromBase64String(base64);

            var galleryItem = new Gallery
            {
                ImageData = bytes,
                ImageMimeType = "image/png"
            };

            _context.Galleries.Add(galleryItem);
            await _context.SaveChangesAsync();

            return Ok(new GalleryDto
            {
                Id = galleryItem.Id,
                ImageBase64 = $"data:{galleryItem.ImageMimeType};base64,{Convert.ToBase64String(galleryItem.ImageData)}"
            });
        }

        // UPDATE gallery image
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GalleryCreateDto request)
        {
            var item = await _context.Galleries.FindAsync(id);
            if (item == null)
                return NotFound();

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                string base64 = request.ImageBase64;
                if (base64.Contains(",")) base64 = base64.Split(',')[1];

                item.ImageData = Convert.FromBase64String(base64);
                item.ImageMimeType = "image/png";
            }

            await _context.SaveChangesAsync();

            return Ok(new GalleryDto
            {
                Id = item.Id,
                ImageBase64 = $"data:{item.ImageMimeType};base64,{Convert.ToBase64String(item.ImageData)}"
            });
        }

        // DELETE gallery image
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Galleries.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.Galleries.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        // GET: return ONLY product images
        [HttpGet("allImage")]
        public async Task<IActionResult> GetProductImages()
        {
            var images = await _context.Products
                .Where(p => p.ImageData != null)
                .Select(p => new GalleryDto
                {
                    Id = p.Id,
                    ImageBase64 = $"data:{p.ImageMimeType};base64,{Convert.ToBase64String(p.ImageData)}"
                })
                .ToListAsync();

            // --- 2) Get gallery images ---
            var galleryImages = await _context.Galleries
                .Where(g => g.ImageData != null)
                .Select(g => new GalleryDto
                {
                    Id = g.Id,
                    ImageBase64 = $"data:{g.ImageMimeType};base64,{Convert.ToBase64String(g.ImageData)}"
                })
                .ToListAsync();

            // --- 3) Merge both ---
            var allImages = images.Concat(galleryImages).ToList();

            return Ok(new
            {
                totalCount = allImages.Count,
                data = allImages
            });
        }
    }
}
