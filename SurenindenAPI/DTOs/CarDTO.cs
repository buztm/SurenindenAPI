namespace SurenindenAPI.DTOs
{
    public class CarDTO
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal DailyPrice { get; set; }
        public string Plate { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public string? ImagePath { get; set; }
        public bool IsAvailable { get; set; }

        public string? CategoryName { get; set; }
    }
}
