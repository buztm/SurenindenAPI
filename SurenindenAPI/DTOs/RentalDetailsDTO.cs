namespace SurenindenAPI.DTOs
{
    public class RentalDetailDTO
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CarInfo { get; set; }
        public string Plate { get; set; }
        public string AppUserId { get; set; }
        public string UserName { get; set; } 
        public string UserEmail { get; set; }
        public DateTime RentDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}