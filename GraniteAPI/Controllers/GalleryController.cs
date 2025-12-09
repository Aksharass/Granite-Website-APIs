using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Migrations;
using GraniteAPI.Models;
using GraniteAPI.Services;
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
                    ImageUrl = g.ImageUrl
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount = gallery.Count,
                data = gallery
            });
        }

        // INSERT gallery image (no productId)
        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] GalleryCreateDto request, [FromServices] CloudinaryService cloudinary)
        {
            if (string.IsNullOrEmpty(request.ImageBase64))
                return BadRequest(new { message = "ImageBase64 is required" });

            // Upload to Cloudinary  
            string imageUrl = await cloudinary.UploadBase64ImageAsync(request.ImageBase64);

            var galleryItem = new Gallery
            {
                ImageUrl = imageUrl
            };

            _context.Galleries.Add(galleryItem);
            await _context.SaveChangesAsync();

            return Ok(new GalleryDto
            {
                Id = galleryItem.Id,
                ImageUrl = galleryItem.ImageUrl
            });
        }

        // UPDATE gallery image
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GalleryCreateDto request, [FromServices] CloudinaryService cloudinary)
        {
            var item = await _context.Galleries.FindAsync(id);
            if (item == null)
                return NotFound();

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                string newImageUrl = await cloudinary.UploadBase64ImageAsync(request.ImageBase64);

                item.ImageUrl = newImageUrl;
            }

            await _context.SaveChangesAsync();

            return Ok(new GalleryDto
            {
                Id = item.Id,
                ImageUrl = item.ImageUrl
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
                .Where(p => p.ImageUrl != null)
                .Select(p => new GalleryDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            // --- 2) Get gallery images ---
            var galleryImages = await _context.Galleries
                .Where(g => g.ImageUrl != null)
                .Select(g => new GalleryDto
                {
                    Id = g.Id,
                    ImageUrl = g.ImageUrl
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
