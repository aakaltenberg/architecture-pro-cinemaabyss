namespace EventsService.Models
{
    public class MovieEvent
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = "";
        public string Action { get; set; } = "";
        public int? UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
