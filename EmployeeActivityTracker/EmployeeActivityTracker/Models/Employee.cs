using System.Collections.Generic;

namespace EmployeeActivityTracker.Models
{
    public enum Sex
    {
        Male,
        Female
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Sex Sex { get; set; }
    }
}
