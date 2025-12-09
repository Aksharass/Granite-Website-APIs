namespace GraniteAPI.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageMimeType { get; set; }

    }
}
