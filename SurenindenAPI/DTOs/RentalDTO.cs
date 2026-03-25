namespace SurenindenAPI.DTOs
{
    public class RentalDTO
    {
        public int CarId { get; set; }
        public string AppUserId { get; set; }
        public DateTime RentDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
