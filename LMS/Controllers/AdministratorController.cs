using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            var dep = from d in db.Departments
                      where d.Subject == subject
                      select d.Subject;
            if (dep.Count() != 0) {// false if the department already exists
                return Json(new { success = false });
            }

            Department department = new Department();
            department.Name = name;
            department.Subject = subject;
            db.Departments.Add(department);
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var courses =
            from c in db.Courses
            where c.Department == subject
            select new { number = c.Number, name = c.Name };

            return Json(courses.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var professors =
            from p in db.Professors
            where p.WorksIn == subject
            select new { lname = p.LName, fname = p.FName, uid = p.UId };

            return Json(professors.ToArray());

        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            // false if the course already exists
            var existingCourses =
            from c in db.Courses
            where c.Department == subject && c.Number == number
            select c.CatalogId;
            if (existingCourses.Count() != 0) {
                return Json(new { success = false });
            }
            

            int cID = (from c in db.Courses
                       orderby c.CatalogId descending
                       select c.CatalogId).Count();
            Course course = new Course();
            //course.CatalogId = (uint)(cID+1);
            course.Department = subject;
            course.Name = name;
            course.Number = (uint)number;
            db.Courses.Add(course);
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception e){
                Console.WriteLine(e.ToString());
                return Json(new { success = false });
            }
            
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            //there is already a Class offering of the same Course in the same Semester,

            uint listing = (from c in db.Courses
                          where c.Department == subject && c.Number == number
                          select c.CatalogId).First();
            var classes = from c in db.Classes.Where(x => x.Listing == listing && x.Year == year && x.Season == season)
                          select c.ClassId;
            if (classes.Count() != 0) {
                return Json(new { success = false });
            }

            //if another class occupies the same location during any time 
            //within the start-end range in the same semester

            var classesAtSameLocation = from cls in db.Classes
                                        where cls.Location == location && cls.Season == season
                                        select new { cls.StartTime, cls.EndTime };
            foreach (var c in classesAtSameLocation) {
                int startToStart = (int) (TimeOnly.FromDateTime(start) - c.StartTime).TotalSeconds;
                int endToStart = (int)(TimeOnly.FromDateTime(end) - c.StartTime).TotalSeconds;
                int startToEnd = (int)(TimeOnly.FromDateTime(start) - c.EndTime).TotalSeconds;
                int endToEnd = (int)(TimeOnly.FromDateTime(end) - c.EndTime).TotalSeconds;
                //start at the same time
                if (startToStart == 0) {
                    return Json(new { success = false });
                }
                else if (endToStart > 0 && endToEnd < 0) {// start earlier but end after the start time of existing class
                    return Json(new { success = false });
                }
                else if (startToStart > 0 && startToEnd < 0) {// start during an existing class is using the room
                    return Json(new { success = false });
                }
            }
            int cID = (from c in db.Classes
                       select c.ClassId).Count() + 1;
            Class newClass = new Class();
            newClass.Season = season;
            newClass.Year = (uint)year;
            newClass.Location = location;
            newClass.StartTime = TimeOnly.FromDateTime(start);
            newClass.EndTime = TimeOnly.FromDateTime(end);
            newClass.Listing = listing;
            newClass.TaughtBy = instructor;

            db.Classes.Add(newClass);
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return Json(new { success = false });
            }
        }


        /*******End code to modify********/

    }
}

