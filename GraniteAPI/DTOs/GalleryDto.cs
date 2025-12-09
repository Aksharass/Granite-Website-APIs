namespace GraniteAPI.DTOs
{
    public class GalleryDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class GalleryCreateDto
    {
        public string ImageBase64 { get; set; } = string.Empty; // only this needed
    }
}
