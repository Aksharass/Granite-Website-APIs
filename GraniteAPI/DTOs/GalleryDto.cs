namespace GraniteAPI.DTOs
{
    public class GalleryDto
    {
        public int Id { get; set; }
        public string? ImageBase64 { get; set; }
    }

    public class GalleryCreateDto
    {
        public string? ImageBase64 { get; set; } // only this needed
    }
}
