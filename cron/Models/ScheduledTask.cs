namespace cron.Models
{
    public class ScheduledTask
    {
        public string Url { get; set; }
        public int IntervalMinutes { get; set; } // how often to trigger
        public DateTime LastFired { get; set; }
    }
}
