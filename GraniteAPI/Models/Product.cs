namespace GraniteAPI.Models

{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageFileName { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
