using System;

namespace EmployeeActivityTracker.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int HoursOfWork { get; set; }

        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int? ProjectId { get; set; }
        public Project Project { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }

        public int? ActivityTypeId { get; set; }
        public ActivityType ActivityType { get; set; }
    }
}
