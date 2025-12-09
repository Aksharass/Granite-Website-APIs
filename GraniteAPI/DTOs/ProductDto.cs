namespace GraniteAPI.DTOs
{
    // Response DTO
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
        public int? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public string? ImageBase64 { get; set; }
    }

    // Create & Update DTO
    public class ProductCreateUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string? ImageBase64 { get; set; }
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
    }
}
