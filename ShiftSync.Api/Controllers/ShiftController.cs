using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftSync.Api.Data;
using ShiftSync.Api.DTOs;
using ShiftSync.Api.Models;
using ShiftSync.Api.Services;

namespace ShiftSync.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ShiftController(AppDbContext context)
        {
            _context = context;

        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftRequest request)
        {
            var shift = new Shift
            {
                Name = request.Name, Status = shiftStatus.Available
            };
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Shift created successfully", Shift = shift });
        }

        //GET: api/shift/available
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableShifts()
        {
            //var shifts = await _context.Shifts.Where(s => s.Status == shiftStatus.Available).ToListAsync();
            var shifts = await _context.Shifts.ToListAsync();
            return Ok(shifts);
        }


        //POST: api/shift/{shiftId}/claim
        [HttpPost("{shiftId}/claim")]
        public async Task<IActionResult> ClaimShift(int shiftId, [FromBody] ClaimShiftRequest request)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if(shift == null)
            {
             return NotFound("Shift Not Found");
            };
            if (shift.Status != shiftStatus.Available)
            {
                return BadRequest("Shift is not available for claiming");
            };
            shift.Status = shiftStatus.Claimed;
            shift.ClaimedAt = DateTime.UtcNow;
            shift.SupervisorId = request.UserId;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Shift claimed successfully", Shift = shift });

        }

        //POST: api/shift/{shiftId}/log
        [HttpPost("{shiftId}/log")]
        public async Task<IActionResult> AddLog(int shiftId, [FromBody] AddLogRequest request)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if(shift == null)
            {
                return NotFound("Shift Not Found");
            }
            if(shift.Status != shiftStatus.Claimed)
            {
                return BadRequest("Shift is not claimed, cannot add log");
            }
            var log = new ShiftLogs
            {
                Description = request.Description,
                LogType = request.Type,
                TimeStamp = DateTime.UtcNow,
                ShiftId = shiftId

            };
            _context.ShiftLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Log added successfully", Log = log });
        }

        [HttpGet("{shiftId}/logs")]
        public async Task<IActionResult> GetShiftLogs(int shiftId)
        {
            var shiftExists = await _context.Shifts.AnyAsync(s => s.Id == shiftId);

            if (!shiftExists) return NotFound("Shift not found.");
            var logs = await _context.ShiftLogs.Where(log => log.ShiftId == shiftId).OrderBy(l => l.TimeStamp).ToListAsync();
           return Ok(logs);
        }

        [HttpPost("{shiftId}/close")]
        public async Task<IActionResult> CloseShift(int shiftId)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if(shift == null)
            {
                return NotFound("Shift Not Found");
            }
            if(shift.Status != shiftStatus.Claimed)
            {
                return BadRequest("Shift is not claimed, cannot close");
            }
            shift.Status = shiftStatus.Closed;
            shift.ClosedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Shift closed successfully", Shift = shift });
        }

        [HttpGet("{shiftId}/report")]
        public async Task<IActionResult> GenerateReport(int shiftId)
        {
            var shift = await _context.Shifts.Include(s => s.ShiftLogs).FirstOrDefaultAsync(s => s.Id == shiftId);
            if(shift == null)
            {
                return NotFound("Shift Not Found");
            }
            if(shift.Status != shiftStatus.Closed)
            {
                return BadRequest("Shift is not closed, cannot generate report");
            }
            // Generate the report using the ShiftReportGenerator service
            var reportBytes = Services.ShiftReportGenerator.GeneratePdf(shift);
            // Return the PDF file as a response
            return File(reportBytes, "application/pdf", $"ShiftHandover_{shift.Name.Replace(" ", "")}_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
    }
}
