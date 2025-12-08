namespace GraniteAPI.DTOs
{
    // ✅ Basic Response DTO
    public class SubCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // category mapping
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    // ✅ Create/Update DTO (Request Model)
    public class SubCategoryCreateUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }

    // ✅ SubCategory with Products DTO
    public class SubCategoryWithProductsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public List<ProductDto> Products { get; set; } = new();
    }
}
