using Microsoft.EntityFrameworkCore;
using ShiftSync.Api.Models;

namespace ShiftSync.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftLogs> ShiftLogs { get; set; }
    }

}
