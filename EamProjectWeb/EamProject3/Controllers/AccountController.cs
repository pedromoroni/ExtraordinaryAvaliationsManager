using EamProject3.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace EamProject3.Controllers
{
    public class AccountController : Controller
    {
        private readonly EamDbContext _context;

        public AccountController(EamDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            HttpContext.Session.SetInt32("LoggedUserId", -1);
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.SetInt32("LoggedUserId", -1);
            return View();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.SetInt32("LoggedUserId", -1);
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string identification, string password)
        {
            if (identification == null || password == null)
            {
                HttpContext.Session.SetInt32("LoggedUserId", -1);
                return RedirectToAction("Login", "Account");
            }
            User? user = _context.Users
                .Where(c => !c.IsDeleted)
                .Include(c => c.Role)
                .Include(c => c.Class)
                .FirstOrDefault(c => c.Identification == identification);

            if (user == null)
            {
                HttpContext.Session.SetInt32("LoggedUserId", -1);
                return RedirectToAction("Login", "Account");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user!.PasswordHash)) 
            {
                HttpContext.Session.SetInt32("LoggedUserId", -1);
                return RedirectToAction("Login", "Account");
            }

            HttpContext.Session.SetInt32("LoggedUserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role.Name); 
            
            if (user.RoleId == 1 && user.Class == null) // aluno não pode entrar sem uma turma
            {
                HttpContext.Session.SetInt32("LoggedUserId", -1);
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Index", user.Role.Name);
        }
    }
}
