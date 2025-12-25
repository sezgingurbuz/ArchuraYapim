using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Kullanıcı zaten giriş yapmışsa tekrar login sayfasına girmesin
            if (User.Identity!.IsAuthenticated)
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
                        new Claim(ClaimTypes.Role, "Admin"), 
                        new Claim("AdminId", admin.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true, // Beni hatırla
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(60) // Oturum süresi
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity), 
                        authProperties);

                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                }
            }

            // Eğer kod buraya düşerse; ya kullanıcı yoktur ya da şifre yanlıştır.
            // Tek bir hata mesajı dönmek güvenlik açısından daha iyidir.
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        public async Task<IActionResult> CikisYap()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        
    }
}