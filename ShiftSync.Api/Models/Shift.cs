

namespace ShiftSync.Api.Models
{
    public enum shiftStatus
    {
        Available = 0,
        Claimed = 1,
        Closed = 2
    }

    public class Shift
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public shiftStatus Status { get; set; } =  shiftStatus.Available; //eg: Available, Claimed, Closed

        public DateTime? ClaimedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public int ? SupervisorId { get; set; }
        public User? Supervisor { get; set; }

        public ICollection<ShiftLogs> ShiftLogs { get; set; } = new List<ShiftLogs>();

    }
}
