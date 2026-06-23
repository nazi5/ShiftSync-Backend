using ShiftSync.Api.Models;

namespace ShiftSync.Api.DTOs
{
    public class AddLogRequest
    {
        public LogType Type { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
