using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var students = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                           join cl in db.Classes.Where(x => x.Season == season && x.Year == year) on c.CatalogId equals cl.Listing into join1

                           from j1 in join1
                           join e in db.Enrolleds on j1.ClassId equals e.Class into join2

                           from j2 in join2
                           join s in db.Students on j2.Student equals s.UId into join3

                           from j3 in join3
                           select new
                           {
                               fname = j3.FName,
                               lname = j3.LName,
                               uid = j3.UId,
                               dob = j3.Dob,
                               grade = j2.Grade
                           };

            return Json(students.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            
            if (category == null)
            {
                var assignments = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                                  join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                                  from j1 in join1
                                  join ac in db.AssignmentCategories on j1.ClassId equals ac.InClass into join2

                                  from j2 in join2
                                  join a in db.Assignments on j2.CategoryId equals a.Category into join3

                                  from j3 in join3
                                  select new
                                  {
                                      aname = j3.Name,
                                      cname = j2.Name,
                                      due = j3.Due,
                                      submissions = (from sub in db.Submissions.Where(x => x.Assignment == j3.AssignmentId)
                                                    select sub.Student).Count()
                                  };


                return Json(assignments.ToArray());
            }
            else {

                var assignments = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                                  join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                                  from j1 in join1
                                  join ac in db.AssignmentCategories.Where(x => x.Name == category) on j1.ClassId equals ac.InClass into join2

                                  from j2 in join2
                                  join a in db.Assignments on j2.CategoryId equals a.Category into join3

                                  from j3 in join3
                                  select new
                                  {
                                      aname = j3.Name,
                                      cname = j2.Name,
                                      due = j3.Due,
                                      submissions = (from sub in db.Submissions.Where(x => x.Assignment == j3.AssignmentId)
                                                    select sub.Student).Count()
                                  };
                return Json(assignments.ToArray());
            }
            
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var assignmentCategories = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                                       join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                                       from j1 in join1
                                       join ac in db.AssignmentCategories on j1.ClassId equals ac.InClass into join2

                                       from j2 in join2
                                       select new
                                       {
                                           name = j2.Name,
                                           weight = j2.Weight
                                       };

            return Json(assignmentCategories.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            uint classID = (from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                           join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                           from j1 in join1
                           select j1.ClassId).First();

            var assignmentCategories = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                                       join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                                       from j1 in join1
                                       join ac in db.AssignmentCategories on j1.ClassId equals ac.InClass into join2

                                       from j2 in join2
                                       where j2.Name == category
                                       select j2.InClass;

            if (assignmentCategories.Count() != 0)
            {
                return Json(new { success = false });
            }
            else {

                AssignmentCategory ac = new AssignmentCategory();
                ac.Name = category;
                ac.Weight = (uint)catweight;
                ac.InClass = classID;
                db.AssignmentCategories.Add(ac);
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
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var assignmentCategories = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                                      join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                                      from j1 in join1
                                      join ac in db.AssignmentCategories on j1.ClassId equals ac.InClass into join2

                                      from j2 in join2
                                      where j2.Name == category
                                      select j2.CategoryId;

            Assignment assignment = new Assignment();
            assignment.Name = asgname;
            assignment.Contents = asgcontents;
            assignment.Due = asgdue;
            assignment.MaxPoints = (uint)asgpoints;
            assignment.Category = assignmentCategories.First();

            db.Assignments.Add(assignment);

            // get class ID for auto grading function
            uint classID = (from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                            join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                            from j1 in join1
                            select j1.ClassId).First();
            try
            {
                db.SaveChanges();
                autoGradingForAllStudent(classID);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var submissions = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                              join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                              from j1 in join1
                              join ac in db.AssignmentCategories on j1.ClassId equals ac.InClass into join2

                              from j2 in join2
                              join a in db.Assignments.Where(x => x.Name == asgname) on j2.CategoryId equals a.Category into join3

                              from j3 in join3
                              join s in db.Submissions on j3.AssignmentId equals s.Assignment into join4

                              from j4 in join4
                              join std in db.Students on j4.Student equals std.UId into join5

                              from j5 in join5
                              select new
                              {
                                  fname = j5.FName,
                                  lname = j5.LName,
                                  uid = j5.UId,
                                  time = j4.Time,
                                  score = j4.Score
                              };

            return Json(submissions.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {

            var submissions = from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                              join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                              from j1 in join1
                              join ac in db.AssignmentCategories on j1.ClassId equals ac.InClass into join2

                              from j2 in join2
                              join a in db.Assignments.Where(x => x.Name == asgname) on j2.CategoryId equals a.Category into join3

                              from j3 in join3
                              select j3.AssignmentId;

            Submission submission = db.Submissions.Single(x => x.Assignment == submissions.First() && x.Student == uid);
            submission.Score = (uint)score;

            //update score
            uint classID = (from c in db.Courses.Where(x => x.Department == subject && x.Number == num)
                            join cl in db.Classes.Where(x => x.Year == year && x.Season == season) on c.CatalogId equals cl.Listing into join1

                            from j1 in join1
                            select j1.ClassId).First();
            try
            {
                db.SaveChanges();
                autoGrading(uid, classID);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return Json(new { success = false });
            }
        }

        /// Auto-Grading for all student in the class after creating a new assignment
        /// <param name="cID">The class id</param>
        public void autoGradingForAllStudent(uint cID) {
            // get all student in the class
            var students = (from e in db.Enrolleds.Where(x => x.Class == cID)
                           select e.Student).ToList();
            foreach (var s in students) {
                autoGrading(s, cID);
            }

        }


        /// Auto-Grading
        /// <param name="uID">The uid of the student who's submission is being graded</param>
        /// <param name="cID">The class id</param>
        public void autoGrading(string uID, uint cID) {

            var categories = (from ac in db.AssignmentCategories.Where(x => x.InClass == cID)
                             select new
                             {
                                 categoryID = ac.CategoryId,
                                 weight = ac.Weight
                             }).ToList();

            double totalGrade = 0.0;
            foreach (var c in categories) {
                var assignments = (from a in db.Assignments.Where(x => x.Category == c.categoryID)
                                 select a.MaxPoints).ToList();
                                 
                var submissions = (from a in db.Assignments.Where(x => x.Category == c.categoryID)
                                   join sub in db.Submissions.Where(x => x.Student == uID) on a.AssignmentId equals sub.Assignment into join1

                                   from j1 in join1
                                   select new
                                   {
                                       score = j1.Score,
                                       max = a.MaxPoints
                                   }).ToList();
                double max = 0;
                double tempSum = 0;
                foreach (var a in assignments) {
                    max += a;
                }
                foreach (var s in submissions) {
                    tempSum += s.score;  
                }
                

                double grade = (double)(tempSum*1.0 / max) * c.weight;
                totalGrade += grade;
            }
            //93-100 A	
            //90-92 A-
            //87-89 B+
            //83-86 B
            //80-82 B-
            //77-79 C+
            //73-76 C
            //70-72 C-
            //67-69 D+
            //63-66 D
            //60-62 D
            //0-59 E

            string[] grades = { "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-" };
            string letterGrade = "";
            if (totalGrade >= 93) {
                letterGrade = "A";
            }
            else if (totalGrade < 60) {
                letterGrade = "E";
            }
            else {
                letterGrade = grades[(int)((totalGrade - 92) * (-1))/3]; 
            }

            Enrolled enrolled = db.Enrolleds.Single(x => x.Student == uID && x.Class == cID);
            enrolled.Grade = letterGrade;

            db.SaveChanges();

        }

        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var taught = from cl in db.Classes.Where(x => x.TaughtBy == uid)
                         join c in db.Courses on cl.Listing equals c.CatalogId into join1

                         from j1 in join1
                         select new
                         {
                             subject = j1.Department,
                             number = j1.Number,
                             name = j1.Name,
                             season = cl.Season,
                             year = cl.Year
                         };
            return Json(taught.ToArray());
        }


        
        /*******End code to modify********/
    }
}

