using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeActivityTracker.Models;
using Serilog;

namespace EmployeeActivityTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly ActivityTrackerContext _context;

        public ActivitiesController(ActivityTrackerContext context)
        {
            _context = context;
            if(!context.Activities.Any())
                Seed(_context);
        }

        // Fills database with test data.
        private static void Seed(ActivityTrackerContext context)
        {
            context.Roles.AddRange(
                new Role
                {
                    Id = 1,
                    Name = "software engineer"
                },
                new Role
                {
                    Id = 2,
                    Name = "software architect"
                },
                new Role
                {
                    Id = 3,
                    Name = "team lead"
                }
            );
            context.ActivityTypes.AddRange(
                    new ActivityType
                    {
                        Id = 1,
                        Name = "regular work"
                    },
                    new ActivityType
                    {
                        Id = 2,
                        Name = "overtime"
                    }
                );
            context.Projects.AddRange(
                    new Project
                    {
                        Id = 1,
                        Name = "Ableton",
                        DateStart = DateTime.Today,
                        DateEnd = DateTime.Today.AddDays(14)
                    },
                    new Project
                    {
                        Id = 2,
                        Name = "Logic Pro",
                        DateStart = DateTime.Today,
                        DateEnd = DateTime.Today.AddDays(10)
                    }
                );
            context.Employees.AddRange(
                    new Employee
                    {
                        Id = 1,
                        Name = "Mykola",
                        Sex = Sex.Male,
                        DateOfBirth = DateTime.Today.AddYears(-20)
                    },
                    new Employee
                    {
                        Id = 2,
                        Name = "Sasha",
                        Sex = Sex.Female,
                        DateOfBirth = DateTime.Today.AddYears(-20)
                    }
                );
            context.Activities.AddRange(
                    new Activity
                    {
                        Id = 1,
                        Date = DateTime.Parse("2020-08-03"),
                        HoursOfWork = 10,
                        EmployeeId = 1,
                        ProjectId = 1,
                        RoleId = 1,
                        ActivityTypeId = 1
                    },
                    new Activity
                    {
                        Id = 2,
                        Date = DateTime.Parse("2020-08-03"),
                        HoursOfWork = 5,
                        EmployeeId = 1,
                        ProjectId = 2,
                        RoleId = 2,
                        ActivityTypeId = 2
                    },
                    new Activity
                    {
                        Id = 3,
                        Date = DateTime.Parse("2020-08-09"),
                        HoursOfWork = 7,
                        EmployeeId = 1,
                        ProjectId = 2,
                        RoleId = 3,
                        ActivityTypeId = 1
                    },
                    new Activity
                    {
                        Id = 4,
                        Date = DateTime.Parse("2020-08-10"),
                        HoursOfWork = 7,
                        EmployeeId = 1,
                        ProjectId = 2,
                        RoleId = 3,
                        ActivityTypeId = 1
                    }
            );
            context.SaveChanges();
        }

        /// <summary>
        /// Returns all recorded Activities.
        /// </summary>
        /// <returns>List of Activities.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
        {
            return await _context.Activities.ToListAsync();
        }

        /// <summary>
        /// Returns activity by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Activity.</returns>
        [HttpGet("activity/{id}")]
        public async Task<ActionResult<Activity>> GetActivity(int id)
        {
            return await _context.Activities.FindAsync(id);
        }

        /// <summary>
        /// Returns all employee`s activities per specified date.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     /1/2020-08-03
        ///
        /// </remarks>
        /// <param name="employeeId"></param>
        /// <param name="date"></param>
        /// <returns>List of Activities.</returns>
        [HttpGet("day/{employeeId}/{date}")]
        public async Task<ActionResult<List<Activity>>> GetActivitiesPerDay(int employeeId, DateTime date)
        {
            try
            {
                return await GetActivitiesPerDayFromContext(employeeId, date).ToListAsync();
            }
            catch (Exception)
            {
                Log.Error("An error occured during GetActivitiesPerDay request.");
            }

            return NotFound();
        }

        /// <summary>
        /// Returns all employee`s activities per specified week.
        /// </summary>
        /// <remarks>
        ///  Sample request:
        ///
        ///     /1/32
        ///
        /// </remarks>
        /// <param name="employeeId"></param>
        /// <param name="weekNumber"></param>
        /// <returns>List of Activities.</returns>
        [HttpGet("week/{employeeId}/{weekNumber}")]
        public async Task<ActionResult<List<Activity>>> GetActivitiesPerWeek(int employeeId, int weekNumber)
        {
            try
            {
                return await GetActivitiesPerWeekFromContext(employeeId, weekNumber).ToListAsync();
            }
            catch (Exception)
            {
                Log.Error("An error occured during GetActivitiesPerWeek request.");
            }

            return NotFound();
        }

        /// <summary>
        /// Returns activities report.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     report/1/2020-08-07
        ///
        /// </remarks>
        /// <param name="employeeId"></param>
        /// <param name="date"></param>
        /// <returns>String with activity report.</returns>
        [HttpGet("report/{employeeId}/{date}")]
        public async Task<ActionResult<String>> GetActivitiesReport(int employeeId, DateTime date)
        {
            try
            {
                var activities = GetActivitiesPerDayFromContext(employeeId, date);
                var employee = await _context.Employees.FindAsync(employeeId);
                var report = new StringBuilder($"{date.Date:yyyy-MM-dd} {employee.Name} ");
                await using (StringWriter writer = new StringWriter(report))
                {
                    foreach (Activity activity in activities)
                    {
                        var employeeRole =
                            (from role in _context.Roles
                                where role.Id == activity.RoleId
                                select role.Name).First();
                        var projectName =
                            (from project in _context.Projects
                                where project.Id == activity.ProjectId
                                select project.Name).First();
                        var activityTypeName =
                            (from activityType in _context.ActivityTypes
                                where activityType.Id == activity.ActivityTypeId
                                select activityType.Name).First();
                        await writer.WriteAsync(
                            $"worked as {employeeRole} on the {projectName} {activity.HoursOfWork} hours {activityTypeName}" +
                            (activity.Equals(activities.Last()) ? "" : "\nand "));
                    }
                }

                return report.ToString();
            }
            catch (Exception)
            {
                Log.Error("An error occured while generating the report.");
            }

            return NotFound();
        }

        /// <summary>
        /// Modifies existing Activity.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="activity"></param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActivity(int id, Activity activity)
        {
            if (id != activity.Id)
            {
                return BadRequest();
            }

            _context.Entry(activity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                Log.Information($"Activity #{activity.Id} was modified.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Adds new Activity.
        /// </summary>
        /// <param name="activity"></param>
        [HttpPost]
        public async Task<ActionResult<Activity>> PostActivity(Activity activity)
        {
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            Log.Information($" Employee #{activity.EmployeeId} added a new activity.");

            return CreatedAtAction("GetActivity", new { id = activity.Id }, activity);
        }

        /// <summary>
        /// Deletes activity.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Activity>> DeleteActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
            Log.Information($"Activity #{activity.Id} was deleted.");

            return activity;
        }

        private bool ActivityExists(int id)
        {
            return _context.Activities.Any(e => e.Id == id);
        }

        // Returns a collection of activities per specified day and employee id.
        private IQueryable<Activity> GetActivitiesPerDayFromContext(int employeeId, DateTime date)
        {
            var activities =
                from activity in _context.Activities
                where activity.EmployeeId == employeeId && activity.Date == date.Date
                select activity;
            return activities;
        }

        // Returns a collection of activities per specified week and employee id.
        private IQueryable<Activity> GetActivitiesPerWeekFromContext(int employeeId, int weekNumber)
        {
            var firstDateOfWeek = FirstDateOfWeek(DateTime.Today.Year, weekNumber);
            var activities =
                from activity in _context.Activities
                where activity.EmployeeId == employeeId && activity.Date >= firstDateOfWeek && activity.Date <= firstDateOfWeek.AddDays(6)
                select activity;
            return activities;
        }

        // Returns date for the given year and number of week.
        private static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            DateTime janOne = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - janOne.DayOfWeek;

            // Use first Thursday in January to get first week of the year as
            // it will never be in Week 52/53
            DateTime firstThursday = janOne.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            // As we're adding days to a date in Week 1,
            // we need to subtract 1 in order to get the right date for week #1
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }

            // Using the first Thursday as starting week ensures that we are starting in the right year
            // then we add number of weeks multiplied with days
            var result = firstThursday.AddDays(weekNum * 7);

            // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
            return result.AddDays(-3);
        }
    }
}
