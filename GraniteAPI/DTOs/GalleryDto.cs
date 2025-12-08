namespace GraniteAPI.DTOs
{
    public class GalleryDto
    {
        public int Id { get; set; }
        public string ImageFileName { get; set; } = string.Empty;
        public string ImageUrl => string.IsNullOrEmpty(ImageFileName)
            ? null
            : $"/images/{ImageFileName}";
    }

    public class GalleryCreateUpdateDto
    {
        public string? ImageBase64 { get; set; } // required for create
    }
}
