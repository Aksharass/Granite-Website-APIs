namespace GraniteAPI.DTOs
{
    // Response DTO
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageFileName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    // Create & Update DTO
    public class ProductCreateUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public IFormFile? Image { get; set; }
        public int CategoryId { get; set; }
    }
}
