namespace EventsService.Models
{
    public class UserEvent
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
