using EamProject3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EamProject3.Helpers;
using System.Net;
using System.Reflection;

namespace EamProject3.Controllers
{
    [NoCache]
    public class TeacherController : Controller
    {
        private readonly EamDbContext _context;

        public TeacherController(EamDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            string profilePicBase64 = loggedUser!.ProfilePic != null
                ? Convert.ToBase64String(loggedUser.ProfilePic)
                : string.Empty;

            var teacherRequests = _context.Requests
                .Where(r => r.TeacherId == loggedUser.Id)
                .Where(r => r.StatusId != 6)
                .Include(r => r.Status)
                .Include(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(r => r.Student)
                .ToList();

            var model = new UserDashboardViewModel
            {
                User = loggedUser,
                Requests = teacherRequests,
                ProfilePictureBase64 = profilePicBase64,
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Requests()
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            string profilePicBase64 = loggedUser!.ProfilePic != null
                ? Convert.ToBase64String(loggedUser.ProfilePic)
                : string.Empty;

            var teacherRequests = _context.Requests
                .Where(r => r.TeacherId == loggedUser.Id)
                .Where(r => r.StatusId != 6)
                .Include(r => r.Status)
                .Include(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(r => r.Student)
                .ToList();

            var model = new UserDashboardViewModel
            {
                User = loggedUser,
                Requests = teacherRequests,
                ProfilePictureBase64 = profilePicBase64,
            };

            return PartialView("_TeacherRequestsPartialView", model);
        }

        public IActionResult ManageRequest(int id)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            int? requestId = id;

            if (requestId == null || requestId == -1)
            {
                return NotFound();
            }

            var request = _context.Requests
                .Include(c => c.Teacher)
                .Include(c => c.Student)
                    .ThenInclude(s => s.Class)
                .Include(c => c.Status)
                .Include(c => c.Course)
                .Include(c => c.Module)
                    .ThenInclude(m => m.Subject)
                .FirstOrDefault(c => c.Id == requestId);

            if (request == null 
                || request.TeacherId != loggedUser!.Id 
                || request.StatusId != 1)
            {
                return NotFound();
            }

            return PartialView("_TeacherManageRequestPartialView", request);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRequest(int id, int teacherId, int durationMin)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            Request? request = _context.Requests
                .Include(c => c.Teacher)
                .FirstOrDefault(c => c.Id == id);

            if (request == null)
            {
                return NotFound();
            }
            if (durationMin < 15 || durationMin > 1440)
            {
                return NotFound();
            }

            if (teacherId == loggedUser!.Id)
            {
                return NotFound();
            }
            if (!request.Teacher.Subjects.Any(c => c.Modules.Contains(request.Module) && !c.IsDeleted))
            {
                return NotFound();
            }

            if (request.StatusId == 1 && request.TeacherId == loggedUser!.Id) 
            {
                request.TeacherId = teacherId;
                request.DurationMin = durationMin;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int id, int durationMin)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            Request? request = await _context.Requests.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }
            if (durationMin < 15 || durationMin > 1440)
            {
                return NotFound();
            }

            if (request.StatusId == 1 && request.TeacherId == loggedUser!.Id) // verifica se é possivel aceitar este request e se é o mesmo professor
            {
                request.StatusId = 3;
                request.DurationMin = durationMin;

                RequestHistory requestHistory = new RequestHistory();
                requestHistory.StatusId = 3;
                requestHistory.UserId = loggedUser.Id;
                requestHistory.Datetime = DateTime.Now;
                requestHistory.RequestId = request.Id;

                _context.RequestHistories.Add(requestHistory);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DenyRequest(int id)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            Request? request = await _context.Requests.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.StatusId == 1 && request.TeacherId == loggedUser!.Id) // verifica se é possivel recusar este request e se é o mesmo professor
            {
                request.StatusId = 2;

                RequestHistory requestHistory = new RequestHistory();
                requestHistory.StatusId = 2;
                requestHistory.UserId = loggedUser.Id;
                requestHistory.Datetime = DateTime.Now;
                requestHistory.RequestId = request.Id;

                _context.RequestHistories.Add(requestHistory);

                await _context.SaveChangesAsync();
            } 

            return RedirectToAction("Index");
        }

        public IActionResult AvailableTeachers(int id) 
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            Subject? subject = _context.Subjects
                .Where(c => !c.IsDeleted)
                .FirstOrDefault(c => c.Id == id);

            if (subject == null)
            {
                return View();
            }

            if (!loggedUser!.Subjects.Contains(subject))
            {
                return NotFound();
            }

            List<User> teachers = _context.Users 
                .Where(c => !c.IsDeleted)
                .Where(c => c.Id != loggedUser!.Id) // confirma se não é o mesmo professor
                .Where(c => c.Role.Id == 2) 
                .Where(c => c.Subjects.Any(c => c.Id == subject.Id))
                .ToList();

            var model = new AllowedTeachersModel
            {
                Teachers = teachers
            };

            return PartialView("_TeacherAllowedTeachersPartialView", model);
        }

        public IActionResult TeacherInformation(int teacherId)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            User? teacher = _context.Users
                .Where(c => !c.IsDeleted)
                .FirstOrDefault(c => c.Id == teacherId);

            if (teacher == null || teacher.RoleId != 2)
            {
                return NotFound();
            }

            return PartialView("_TeacherManageRequestTeacherPartialView", teacher);
        }

        public IActionResult ModuleGrades()
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ModuleGradesModel
            {
                Teacher = loggedUser!,
            };

            return PartialView("_TeacherEnterGradesPartialView", model);
        }

        public IActionResult StudentsGrade(int moduleId, DateTime examDateTime)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }
             
            if (!_context.Modules.Any(c => c.Id == moduleId && !c.IsDeleted)) // verifica se o modulo existe
            {
                return NotFound();
            }
            if (examDateTime > DateTime.Now)
            {
                return NotFound();
            }

            var studentsgrade = new List<StudentGrade>();

            foreach (Request request in _context.Requests
                .Where(c => c.TeacherId == loggedUser!.Id)
                .Where(c => c.Grade == null)
                .Where(c => c.ExamDatetime == examDateTime))
            {
                if (request.ModuleId == moduleId && !studentsgrade.Any(c => c.StudentId == request.StudentId))
                {
                    StudentGrade studentGrade = new StudentGrade();
                    studentGrade.StudentId = request.StudentId;
                    studentGrade.Student = _context.Users.FirstOrDefault(c => c.Id == request.StudentId);
                    studentsgrade.Add(studentGrade);
                }
            }

            return PartialView("_TeacherGradesTablePartialView", studentsgrade);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitGrades(ModuleGradesModel model)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 2))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!_context.Modules.Any(c => c.Id == model.SelectedModule.ModuleId && !c.IsDeleted)) // verifica se o modulo existe
            {
                return NotFound();
            }

            if (model.SelectedModule.ExamDateTime > DateTime.Now)
            {
                return NotFound();
            }

            foreach (Request request in _context.Requests
                .Where(c => c.ModuleId == model.SelectedModule.ModuleId)
                .Where(c => c.TeacherId == loggedUser!.Id)
                .Where(c => c.ExamDatetime <= model.SelectedModule.ExamDateTime))
            {

                StudentGrade? studentGrade = model.StudentGrades
                    .FirstOrDefault(c => c.StudentId == request.StudentId);

                if (studentGrade == null
                    || request.StatusId != 4)
                {
                    continue;
                }
                if (studentGrade.Grade < 1 || studentGrade.Grade > 20)
                {
                    continue;
                }

                request.Grade = studentGrade.Grade;
                request.StatusId = 5;

                RequestHistory requestHistory = new RequestHistory();
                requestHistory.StatusId = 5;
                requestHistory.UserId = loggedUser!.Id;
                requestHistory.Datetime = DateTime.Now;
                requestHistory.RequestId = request.Id;

                _context.RequestHistories.Add(requestHistory);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); 
        }

        private User? GetLoggedUser()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");

            if (loggedUserId == null || loggedUserId == -1)
            {
                return null;
            }

            var loggedUser = _context.Users
                .Where(c => c.IsDeleted == false)
                .Include(c => c.RequestTeachers)
                    .ThenInclude(r => r.Status)
                .Include(c => c.RequestTeachers)
                    .ThenInclude(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(c => c.RequestTeachers)
                    .ThenInclude(r => r.Student)
                .Include(c => c.Subjects)
                .FirstOrDefault(c => c.Id == loggedUserId);

            return loggedUser;
        }
    }
}
