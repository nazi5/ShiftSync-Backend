namespace ShiftSync.Api.Models
{
    public enum LogType
    {
        Manpower = 0,
        Incident = 1,
        Accident = 2
    }

    public class ShiftLogs
    {
        public int Id { get; set; }
        public LogType LogType { get; set; } = LogType.Manpower;
        public string Description { get; set; } = string.Empty;

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }
    }
}
