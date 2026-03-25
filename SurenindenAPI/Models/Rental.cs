namespace SurenindenAPI.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string AppUserId { get; set; }

        public DateTime RentDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal TotalPrice { get; set; }

        public virtual Car Car { get; set; }
    }
}
