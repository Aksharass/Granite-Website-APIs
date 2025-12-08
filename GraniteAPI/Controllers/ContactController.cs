using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraniteAPI.Data;
using GraniteAPI.DTOs;
using GraniteAPI.Models;
using GraniteAPI.Services;

namespace GraniteAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SendGridService _sendGrid;

        public ContactController(ApplicationDbContext context, SendGridService sendGrid)
        {
            _context = context;
            _sendGrid = sendGrid;
        }

        [HttpPost("insert")]
        public async Task<IActionResult> Create([FromBody] ContactCreateDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid request body" });

            try
            {
                // save in database
                var contact = new Contact
                {
                    Name = dto.Name,
                    Company = dto.Company,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Subject = dto.Subject,
                    Message = dto.Message
                };

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                // email body
                string html = $@"
                    <h3>New Contact Request</h3>
                    <p><strong>Name:</strong> {dto.Name}</p>
                    <p><strong>Email:</strong> {dto.Email}</p>
                    <p><strong>Phone:</strong> {dto.PhoneNumber}</p>
                    <p><strong>Company:</strong> {dto.Company}</p>
                    <p><strong>Subject:</strong> {dto.Subject}</p>
                    <p><strong>Message:</strong><br>{dto.Message}</p>
                ";

                // send SendGrid email
                await _sendGrid.SendDynamicEmailAsync(
                    dto.Email,          // dynamic FROM email
                    dto.Name,
                    dto.Subject,
                    html
                );

                return Ok(new { message = "Contact saved and email sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to submit contact", error = ex.Message });
            }
        }
    }
}
