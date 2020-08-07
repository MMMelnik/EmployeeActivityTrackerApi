using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeActivityTracker.Models
{
    public class Activity
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int HoursOfWork { get; set; }
        [Required]
        public int? EmployeeId { get; set; }
        [Required]
        public int? ProjectId { get; set; }
        [Required]
        public int? RoleId { get; set; }
        [Required]
        public int? ActivityTypeId { get; set; }
    }
}
