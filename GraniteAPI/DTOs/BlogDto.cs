namespace GraniteAPI.DTOs
{
    public class BlogDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public string ImageFileName { get; set; } = string.Empty;

        public string ImageUrl => string.IsNullOrEmpty(ImageFileName)
            ? null
            : $"/images/{ImageFileName}";

    }

    public class BlogCreateUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageBase64 { get; set; }
    }
}
