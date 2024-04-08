using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classes = from e in db.Enrolleds.Where(x => x.Student == uid)
                          join cl in db.Classes on e.Class equals cl.ClassId into join1

                          from j1 in join1
                          join c in db.Courses on j1.Listing equals c.CatalogId into join2

                          from j2 in join2
                          select new
                          {
                              subject = j2.Department,
                              number = j2.Number,
                              name = j2.Name,
                              season = j1.Season,
                              year = j1.Year,
                              grade = e.Grade
                          };
                       
            return Json(classes.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var assignments = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                              join cl in db.Classes.Where(x => x.Season == season && x.Year == year) on c.CatalogId equals cl.Listing into join1

                              from j1 in join1
                              join ac in db.AssignmentCategories on j1.ClassId equals ac.InClass into join2

                              from j2 in join2
                              join a in db.Assignments on j2.CategoryId equals a.Category into join3

                              from j3 in join3
                              join s in db.Submissions.Where(x => x.Student == uid) on j3.AssignmentId equals s.Assignment into join4

                              from j4 in join4.DefaultIfEmpty()
                              select new
                              {
                                  aname = j3.Name,
                                  cname = j2.Name,
                                  due = j3.Due,
                                  score = j4.Score == null ? null : (uint?)j4.Score
                              };        
            return Json(assignments.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var assignment = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                             join cl in db.Classes.Where(x => x.Season == season && x.Year == year) on c.CatalogId equals cl.Listing into join1

                             from j1 in join1
                             join ac in db.AssignmentCategories.Where(x => x.Name == category) on j1.ClassId equals ac.InClass into join2

                             from j2 in join2
                             join a in db.Assignments.Where(x => x.Name == asgname) on j2.CategoryId equals a.Category into join3

                             from j3 in join3
                             select j3.AssignmentId;

            var submission = from s in db.Submissions.Where(x => x.Assignment == assignment.First() && x.Student == uid)
                             select s;

            if (submission.Count() == 0)
            {// first submission

                Submission sub = new Submission();
                sub.Student = uid;
                sub.Assignment = assignment.First();
                sub.Score = 0;
                sub.SubmissionContents = contents;
                sub.Time = DateTime.Now;

                db.Submissions.Add(sub);
            }
            else { // re submission

                Submission sub = db.Submissions.Single(x => x.Assignment == assignment.First() && x.Student == uid);
                sub.SubmissionContents = contents;
                sub.Time = DateTime.Now;

            }
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
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var classID = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                          join cl in db.Classes.Where(x => x.Season == season && x.Year == year) on c.CatalogId equals cl.Listing into join1

                          from j1 in join1
                          select j1.ClassId;

            var enroll = from e in db.Enrolleds.Where(x => x.Class == classID.First() && x.Student == uid)
                         select e;

            if (enroll.Count() == 0)
            { // first enroll
                Enrolled enr = new Enrolled();
                enr.Student = uid;
                enr.Class = classID.First();
                enr.Grade = "--";
                db.Enrolleds.Add(enr);
            }
            else { // already enrolled
                return Json(new { success = false });
            }
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
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            string[] grades = { "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-", "E" };
            double[] points = { 4.0, 3.7, 3.3, 3.0, 2.7, 2.3, 2.0, 1.7, 1.3, 1.0, 0.7, 0.0 };

            Dictionary<string, double> gradeToPoints = new Dictionary<string, double>();
            for (int x = 0; x < grades.Length; x++)
            {
                gradeToPoints.Add(grades[x], points[x]);
            }

            var grade = from e in db.Enrolleds.Where(x => x.Student == uid)
                        select e.Grade;
            double total = 0.0;
            int classes = 0;
            if (grade.Count() == 0)
            {// the student is not enrolled in any classes

                return Json(new { gpa = total });
            }

            foreach (string g in grade) {
                if (g == "--")
                {
                    continue;
                }
                else {
                    classes++;
                    total += gradeToPoints[g];
                }
            }

            if (classes == 0)
            {
                return Json(new { gpa = total });
            }
            else {

                return Json(new { gpa = (total/classes) });
            }
        }
                
        /*******End code to modify********/

    }
}

