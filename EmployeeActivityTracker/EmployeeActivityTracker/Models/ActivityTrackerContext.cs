using Microsoft.EntityFrameworkCore;

namespace EmployeeActivityTracker.Models
{
    public class ActivityTrackerContext: DbContext
    {
        public ActivityTrackerContext(DbContextOptions<ActivityTrackerContext> options)
            : base(options)
        {
            
        }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityType> ActivityTypes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public  DbSet<Role> Roles { get; set; }
    }
}
