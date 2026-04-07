namespace EventsService.Models
{
    public class PaymentEvent
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string MethodType { get; set; } = "";
    }
}
