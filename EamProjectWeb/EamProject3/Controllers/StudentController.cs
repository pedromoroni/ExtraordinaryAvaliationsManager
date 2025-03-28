using EamProject3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Module = EamProject3.Models.Module;
using EamProject3.Helpers;

namespace EamProject3.Controllers
{
    [NoCache]
    public class StudentController : Controller
    {
        private readonly EamDbContext _context;

        public StudentController(EamDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            string profilePicBase64 = loggedUser!.ProfilePic != null
                ? Convert.ToBase64String(loggedUser.ProfilePic)
                : string.Empty;

            var studentRequests = _context.Requests
                .Where(r => r.StudentId == loggedUser.Id)
                .Include(r => r.Status)
                .Include(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(r => r.Teacher)
                .ToList();

            var model = new UserDashboardViewModel
            {
                User = loggedUser,
                Requests = studentRequests,
                ProfilePictureBase64 = profilePicBase64,
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult NewRequest()
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            Subject? subject = _context.Subjects
                .Where(c => !c.IsDeleted)
                .Where(c => c.Courses.Any(c => c.Id == loggedUser!.Class!.CourseId))
                .FirstOrDefault();

            if (subject == null)
            {
                return NotFound();
            }

            List<Module> modules = _context.Modules
                .Where(c => !c.IsDeleted)
                .Where(c => c.SubjectId == subject.Id)
                .Where(c => !c.Requests.Any(r => r.StudentId == loggedUser!.Id
                                            && (r.StatusId != 2
                                                && r.StatusId != 5
                                                && r.StatusId != 6)))
                .ToList();

            List<User> teachers = _context.Users
                .Where(c => !c.IsDeleted)
                .Where(c => c.Role.Id == 2)
                .Where(c => c.Subjects.Any(c => c.Id == subject.Id))
                .ToList();

            var model = new CreateNewRequestModel
            {
                Course = loggedUser!.Class!.Course,
                Modules = modules,
                Teachers = teachers
            };

            return PartialView("_StudentNewRequestPartialView", model);
        }

        [HttpGet]
        public IActionResult Requests()
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            string profilePicBase64 = loggedUser!.ProfilePic != null
                ? Convert.ToBase64String(loggedUser.ProfilePic)
                : string.Empty;

            var studentRequests = _context.Requests
                .Where(r => r.StudentId == loggedUser.Id)
                .Include(r => r.Status)
                .Include(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(r => r.Teacher)
                .ToList();

            var model = new UserDashboardViewModel
            {
                User = loggedUser,
                Requests = studentRequests,
                ProfilePictureBase64 = profilePicBase64,
            };

            return PartialView("_StudentRequestsPartialView", model);
        }

        public IActionResult NewRequestModules(int subjectId) 
        {
            Subject? subject = _context.Subjects
                .Where(c => !c.IsDeleted)
                .FirstOrDefault(c => c.Id == subjectId);

            if (subject == null)
            {
                return View();
            }

            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!loggedUser!.Class!.Course.Subjects.Contains(subject)) // verifica se o subject é valido para o aluno
            {
                return NotFound();
            }

            List<Module> modules = _context.Modules
                .Where(c => !c.IsDeleted)
                .Where(c => c.SubjectId == subjectId) 
                .Where(c => !c.Requests.Any(r => r.StudentId == loggedUser!.Id
                                            && (r.StatusId != 2
                                                && r.StatusId != 5
                                                && r.StatusId != 6)))
                .ToList();

            var model = new CreateNewRequestModel
            {
                Course = loggedUser!.Class!.Course,
                Modules = modules,
            };

            return PartialView("_StudentNewRequestsModules", model);
        }

        public IActionResult NewRequestTeachers(int subjectId) 
        {
            Subject? subject = _context.Subjects
                .Where(c => !c.IsDeleted)
                .FirstOrDefault(c => c.Id == subjectId);

            if (subject == null)
            {
                return View(); 
            }

            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!loggedUser!.Class!.Course.Subjects.Contains(subject)) // verifica se o subject é valido para o aluno
            {
                return NotFound();
            }

            List<User> teachers = _context.Users
                .Where(c => !c.IsDeleted)
                .Where(c => c.Role.Id == 2)
                .Where(c => c.Subjects.Any(c => c.Id == subject.Id))
                .ToList();


            var model = new CreateNewRequestModel
            {
                Course = loggedUser.Class.Course,
                Teachers = teachers
            };

            return PartialView("_StudentNewRequestTeachers", model);
        }

        [HttpPost] 
        public async Task<IActionResult> CreateNewRequest(CreateNewRequestModel createNewRequestModel)
        {
            Request newRequest = new Request();

            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            Module? module = _context.Modules
                .Where(c => !c.IsDeleted)
                .FirstOrDefault(m => m.Id == createNewRequestModel.ModuleId);

            User? teacher = _context.Users
                .Where(c => !c.IsDeleted)
                .Include(c => c.Subjects)
                    .ThenInclude(s => s.Modules)
                .FirstOrDefault(c => c.Id == createNewRequestModel.TeacherId);

            if (module == null || teacher == null)
            {
                return NotFound();
            }
            if (!loggedUser!.Class!.Course.Subjects.Any(c => c.Modules.Contains(module))
                || !teacher!.Subjects.Any(c => c.Modules.Contains(module))) // verifica se o modulo é válido para este estudante e para o professor
            {
                return NotFound();
            }
            if (createNewRequestModel.DurationMin < 15 
                || createNewRequestModel.DurationMin > 1440)
            {
                return NotFound();
            }
            if (_context.Requests
                .Where(r => r.StudentId == loggedUser!.Id && r.ModuleId == createNewRequestModel.ModuleId)
                .Any(r => r.StatusId != 2 && r.StatusId != 5 && r.StatusId != 6)) // verificar se o modulo nao é repetido
            {
                return NotFound();
            }

            string number = (loggedUser!.RequestStudents.Count() + 1).ToString("00") + '/' + loggedUser!.Identification.ToString();

            newRequest.Number = number; 
            newRequest.StudentId = loggedUser!.Id;
            newRequest.StatusId = 1;
            newRequest.CourseId = loggedUser!.Class!.Course.Id;
            newRequest.ModuleId = createNewRequestModel.ModuleId;
            newRequest.TeacherId = createNewRequestModel.TeacherId;
            newRequest.ExamDatetime = createNewRequestModel.ExamDateTime;
            newRequest.DurationMin = createNewRequestModel.DurationMin;
            newRequest.SituationId = _context.Situations
                .Where(c => !c.IsDeleted)
                .FirstOrDefault(c => c.StartAt <= DateOnly.FromDateTime(createNewRequestModel.ExamDateTime)
                                  && c.EndAt <= DateOnly.FromDateTime(createNewRequestModel.ExamDateTime))?.Id ?? _context.Situations.First(c => !c.IsDeleted).Id;

            _context.Requests.Add(newRequest);
            await _context.SaveChangesAsync();

            Request request = _context.Requests.First(c => c.Number == number);

            RequestHistory requestHistory = new RequestHistory();
            requestHistory.StatusId = 1;
            requestHistory.UserId = loggedUser.Id;
            requestHistory.Datetime = DateTime.Now;
            requestHistory.RequestId = newRequest.Id;

            _context.RequestHistories.Add(requestHistory);  

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);

            if (request == null)
            {
                return Requests();
            }

            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            if (request.StatusId == 1 && request.StudentId == loggedUser!.Id) // verifica se o estudante tem permissão de cancelar
            {
                request.StatusId = 6;

                RequestHistory requestHistory = new RequestHistory();
                requestHistory.StatusId = 6;
                requestHistory.UserId = loggedUser!.Id;
                requestHistory.Datetime = DateTime.Now;
                requestHistory.RequestId = request.Id;
                _context.RequestHistories.Add(requestHistory);

                await _context.SaveChangesAsync();
            }

            string profilePicBase64 = loggedUser!.ProfilePic != null
                ? Convert.ToBase64String(loggedUser.ProfilePic)
                : string.Empty;

            var studentRequests = _context.Requests
                .Where(r => r.StudentId == loggedUser.Id)
                .Include(r => r.Status)
                .Include(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(r => r.Teacher)
                .ToList();

            var model = new UserDashboardViewModel
            {
                User = loggedUser,
                Requests = studentRequests,
                ProfilePictureBase64 = profilePicBase64,
            };

            return PartialView("_StudentRequestsPartialView", model);
        }

        public IActionResult PaidRequestInfo(int id)
        {
            var request = _context.Requests
                .Include(c => c.Situation)
                .FirstOrDefault(c => c.Id == id);

            if (request == null)
            {
                return Requests();
            }

            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            if (request.StudentId != loggedUser!.Id
                || (request.StatusId != 4 && request.StatusId != 5))
            {
                return NotFound();
            }
            return PartialView("_StudentPaidRequest", request);
        }

        public IActionResult Grades()
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 1))
            {
                return RedirectToAction("Login", "Account");
            }

            List<Request> requests = _context.Requests
                .Where(c => c.StudentId == loggedUser!.Id)
                .Where(c => c.StatusId == 5)
                .Include(c => c.Teacher)
                .ToList();

            return PartialView("_StudentGradesPartialView", requests);
        }

        private User? GetLoggedUser()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");

            if (loggedUserId == null || loggedUserId == -1)
            {
                return null;
            }

            var loggedUser = _context.Users
                .Where(c => !c.IsDeleted)
                .Include(c => c.RequestStudents)
                    .ThenInclude(r => r.Status)
                .Include(c => c.RequestStudents)
                    .ThenInclude(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(c => c.RequestStudents)
                    .ThenInclude(r => r.Teacher)
                .Include(c => c.Class)
                    .ThenInclude(cl => cl.Course)
                    .ThenInclude(course => course.Subjects)
                .FirstOrDefault(c => c.Id == loggedUserId);

            return loggedUser;
        }

    }
}