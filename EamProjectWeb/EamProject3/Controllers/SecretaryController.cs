using EamProject3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EamProject3.Helpers;
using System.Net;

namespace EamProject3.Controllers
{
    [NoCache]
    public class SecretaryController : Controller
    {
        private readonly EamDbContext _context;

        public SecretaryController(EamDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 3))
            {
                return RedirectToAction("Login", "Account");
            }

            string profilePicBase64 = loggedUser!.ProfilePic != null
                ? Convert.ToBase64String(loggedUser.ProfilePic)
                : string.Empty;

            var requests = _context.Requests
                .Include(r => r.Status)
                .Include(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(r => r.Student)
                .ToList();

            var model = new UserDashboardViewModel
            {
                User = loggedUser,
                Requests = requests,
                ProfilePictureBase64 = profilePicBase64,
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Requests()
        {
           User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 3))
            {
                return RedirectToAction("Login", "Account");
            }

            string profilePicBase64 = loggedUser!.ProfilePic != null
                ? Convert.ToBase64String(loggedUser.ProfilePic)
                : string.Empty;

            var requests = _context.Requests 
                .Include(r => r.Status)
                .Include(r => r.Module)
                    .ThenInclude(m => m.Subject)
                .Include(r => r.Student)
                .ToList();

            var model = new UserDashboardViewModel
            {
                User = loggedUser,
                Requests = requests,
                ProfilePictureBase64 = profilePicBase64,
            };

            return PartialView("_SecretaryRequestsPartialView", model);
        }

        public IActionResult ManageRequest(int id)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 3))
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

            if (request == null)
            {
                return NotFound();
            }

            var model = new RequestSituationModel
            {
                Request = request,
                Situations = _context.Situations.Where(c => !c.IsDeleted).ToList()
            };

            return PartialView("_SecretaryManageRequestPartialView", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRequest(int id, string paymentMethod, int situationId)
        {
            User? loggedUser = GetLoggedUser();

            if (!Helper.VerifyUser(loggedUser, 3))
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

            string[] paymentMethods = {
                "Cash",
                "Debit/Credit Card",
                "MB Way",
                "PayPal"
            };

            if (!paymentMethods.Contains(paymentMethod))
            {
                return NotFound();
            }


            if (request.StatusId == 3) 
            {
                request.PaymentMethod = paymentMethod;
                request.StatusId = 4;
                if (_context.Situations.Any(c => c.Id == situationId && c.StartAt == null))
                {
                    request.SituationId = situationId;
                }

                RequestHistory requestHistory = new RequestHistory();
                requestHistory.StatusId = 4;
                requestHistory.UserId = loggedUser!.Id;
                requestHistory.Datetime = DateTime.Now;
                requestHistory.RequestId = request.Id;

                _context.RequestHistories.Add(requestHistory);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
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

            if (!Helper.VerifyUser(loggedUser, 3))
            {
                return RedirectToAction("Login", "Account");
            }

            if (request.StatusId != 4 && request.StatusId != 5)
            {
                return NotFound();
            }
            return PartialView("_SecretaryPaidRequest", request);
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
                .FirstOrDefault(c => c.Id == loggedUserId);

            return loggedUser;
        }
    }
}
