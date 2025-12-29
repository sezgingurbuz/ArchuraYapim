using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using basics.Data;
using basics.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization; 

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string AdminScheme = "AdminScheme";

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Admin oturumu açık mı kontrol et
            var authResult = await HttpContext.AuthenticateAsync(AdminScheme);
            if (authResult.Succeeded)
            {
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string userName, string password)
        {
            // Kullanıcıyı bul
            var admin = _context.AdminUsers.FirstOrDefault(x => x.userName == userName);

            if (admin != null)
            {
                // Şifreyi doğrula
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, admin.passwordHash);

                if (isPasswordValid)
                {
                    // Kimlik Bilgilerini (Claims) Hazırla
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.userName),
                        new Claim(ClaimTypes.Role, admin.userName.ToLower() == "admin" ? "Admin" : admin.Role),
                        new Claim("AdminId", admin.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, AdminScheme);
                    
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddHours(8)
                    };

                    // AdminScheme ile giriş yap
                    await HttpContext.SignInAsync(
                        AdminScheme, 
                        new ClaimsPrincipal(claimsIdentity), 
                        authProperties);

                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                }
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        public async Task<IActionResult> CikisYap()
        {
            await HttpContext.SignOutAsync(AdminScheme);
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}