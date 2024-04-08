using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var departments = from d in db.Departments
                              select new { name = d.Name, d.Subject };
            return Json(departments.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {

            var ret = from d in db.Departments
                      select new
                      {
                          subject = d.Subject,
                          dname = d.Name,
                          courses = (from c in db.Courses.Where(x => x.Department == d.Subject)
                                    select new { number = c.Number, cname = c.Name }).ToArray()

                      };

            return Json(ret.ToArray());

        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var classes = from c in db.Courses.Where(x => x.Department == subject && x.Number == number)
                          join cl in db.Classes on c.CatalogId equals cl.Listing into join1
                          from j1 in join1
                          join p in db.Professors on j1.TaughtBy equals p.UId into join2
                          from j2 in join2
                          select new
                          {
                              season = j1.Season,
                              year = j1.Year,
                              location = j1.Location,
                              start = j1.StartTime,
                              end = j1.EndTime,
                              fname = j2.FName,
                              lname = j2.LName
                          };

            return Json(classes.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {

            var query = (from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                         join cl in db.Classes.Where(x => x.Season == season && x.Year == year) on c.CatalogId equals cl.Listing into join1

                         from j1 in join1.DefaultIfEmpty()
                         join ac in db.AssignmentCategories.Where(x => x.Name == category) on j1.ClassId equals ac.InClass into join2

                         from j2 in join2.DefaultIfEmpty()
                         join a in db.Assignments.Where(x => x.Name == asgname) on j2.CategoryId equals a.Category into join3

                         from j3 in join3.DefaultIfEmpty()
                         select j3.Contents).First();

            return Content(query);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var query = (from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                        join cl in db.Classes.Where(x => x.Season == season && x.Year == year) on c.CatalogId equals cl.Listing into join1

                        from j1 in join1
                        join ac in db.AssignmentCategories.Where(x => x.Name == category) on j1.ClassId equals ac.InClass into join2

                        from j2 in join2
                        join a in db.Assignments.Where(x => x.Name == asgname) on j2.CategoryId equals a.Category into join3

                        from j3 in join3.DefaultIfEmpty()
                        join s in db.Submissions.Where(x => x.Student == uid) on j3.AssignmentId equals s.Assignment into join4

                        from j4 in join4.DefaultIfEmpty()
                        select j4.SubmissionContents).First();



            return Content(query);
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var student = from s in db.Students where s.UId == uid select s.Major;
            var professor = from p in db.Professors where p.UId == uid select p.WorksIn;
            var admin = from a in db.Administrators where a.UId == uid select a.UId;

            bool isStudent = false;
            bool isProfessor = false;
            bool isAdmin = false;

            if (student.Count() != 0) isStudent = true;
            if (professor.Count() != 0) isProfessor = true;
            if (admin.Count() != 0) isAdmin = true;

            if (isStudent)
            {
                var ret = from s in db.Students
                          where s.UId == uid
                          select new { fname = s.FName, lname = s.LName, uid = s.UId, department = s.Major };
                return Json(ret.ToArray()[0]);
            }
            else if (isProfessor)
            {
                var ret = from p in db.Professors
                          where p.UId == uid
                          select new { fname = p.FName, lname = p.LName, uid = p.UId, department = p.WorksIn };
                return Json(ret.ToArray()[0]);
            }
            else if (isAdmin)
            {
                var ret = from a in db.Administrators
                          where a.UId == uid
                          select new { fname = a.FName, lname = a.LName, uid = a.UId };
                return Json(ret.ToArray()[0]);
            }
            else {
                return Json(new { success = false });
            }
        }


        /*******End code to modify********/
    }
}

