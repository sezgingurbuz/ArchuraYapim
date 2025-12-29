using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using basics.Data;
using basics.Models;

namespace basics.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private const string CustomerScheme = "CustomerScheme";

    public AccountController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #region Login

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        // Müşteri oturumu açık mı kontrol et
        var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
        if (authResult.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        
        ViewData["Title"] = "Giriş Yap";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "E-posta ve şifre gereklidir.";
            return View();
        }

        // Kullanıcıyı bul
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);

        if (user != null && user.IsActive)
        {
            // Şifreyi doğrula
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (isPasswordValid)
            {
                // Kimlik bilgilerini hazırla
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CustomerScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync(
                    CustomerScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
        }

        ViewBag.Error = "E-posta veya şifre hatalı!";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LoginAjax(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return Json(new { success = false, message = "E-posta ve şifre gereklidir." });
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);

        if (user != null && user.IsActive)
        {
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (isPasswordValid)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CustomerScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(CustomerScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return Json(new { success = true });
            }
        }

        return Json(new { success = false, message = "E-posta veya şifre hatalı!" });
    }

    #endregion

    #region Register

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        // Müşteri oturumu açık mı kontrol et
        var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
        if (authResult.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Kayıt Ol";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string firstName, string lastName, string email, string phoneNumber, string password, string confirmPassword)
    {
        ViewData["Title"] = "Kayıt Ol";

        // Validation
        if (string.IsNullOrWhiteSpace(firstName))
        {
            ViewBag.Error = "Ad gereklidir.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            ViewBag.Error = "Soyad gereklidir.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ViewBag.Error = "E-posta gereklidir.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            ViewBag.Error = "Telefon numarası gereklidir.";
            return View();
        }

        if (phoneNumber.Length != 10 || !phoneNumber.All(char.IsDigit))
        {
            ViewBag.Error = "Telefon numarası 10 haneli olmalıdır.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Şifre gereklidir.";
            return View();
        }

        if (password.Length < 6)
        {
            ViewBag.Error = "Şifre en az 6 karakter olmalıdır.";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Şifreler eşleşmiyor.";
            return View();
        }

        // E-posta kontrolü
        if (_dbContext.Users.Any(u => u.Email == email))
        {
            ViewBag.Error = "Bu e-posta adresi zaten kayıtlı.";
            return View();
        }

        // Telefon numarasını +90 ile birleştir
        var fullPhoneNumber = $"+90{phoneNumber.Trim()}";

        // Yeni kullanıcı oluştur
        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLower(),
            PhoneNumber = fullPhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Otomatik giriş yap
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim("UserId", user.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CustomerScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CustomerScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        TempData["Success"] = "Hesabınız başarıyla oluşturuldu!";
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Logout

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CustomerScheme);
        return RedirectToAction("Index", "Home");
    }

    #endregion
}
