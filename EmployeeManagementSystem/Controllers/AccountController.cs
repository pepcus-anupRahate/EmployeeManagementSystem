using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    public class AccountController(IJwtTokenService jwtTokenService) : Controller
    {
        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "password")
            {
                var token = _jwtTokenService.GenerateToken(username, "Admin");

                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30) // Expires in 30 minutes
                });
                return RedirectToAction("Index", "Employee");
            }
            return Unauthorized();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet("/Account/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult SecurePage()
        {
            return Content("Welcome to the secure page!");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPage()
        {
            return Content("Welcome to the admin page!");
        }
    }
}
