namespace GraniteAPI.DTOs
{
    // ✅ Response DTO (includes list of ProductDto)
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

       
    }

    // ✅ Create/Update DTO (request model)
    public class CategoryCreateUpdateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CategoryWithSubCategoriesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<SubCategoryDto> SubCategories { get; set; } = new();
    }

    public class CategoryFullDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<ProductDto> Products { get; set; } = new();
        public List<SubCategoryDto> SubCategories { get; set; } = new();
    }
}
