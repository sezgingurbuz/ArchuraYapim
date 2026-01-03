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

    [HttpGet]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action("GoogleResponse", "Account");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CustomerScheme);
        if (!result.Succeeded)
        {
            // Google'dan bilgi alınamadı
            ViewBag.Error = "Google ile giriş yapılırken bir hata oluştu.";
            return View("Login");
        }

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var firstName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? name?.Split(' ')[0];
        var lastName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? (name?.Contains(' ') == true ? name[(name.IndexOf(' ')+1)..] : "");

        if (string.IsNullOrEmpty(email))
        {
             ViewBag.Error = "Google hesabınızdan e-posta bilgisi alınamadı.";
             return View("Login");
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            // Yeni kullanıcı oluştur
            user = new User
            {
                Email = email,
                FirstName = firstName ?? "Google",
                LastName = lastName ?? "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Rastgele şifre
                PhoneNumber = "+900000000000" // Zorunlu alan olduğu için placeholder
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        // Kullanıcı girişini yap (Kendi scheme'imizle)
        var appClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim("UserId", user.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(appClaims, CustomerScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddDays(30)
        };

        // Önceki geçici Google cookie'sini silip kendi cookie'mizi oluşturuyoruz
        await HttpContext.SignOutAsync(CustomerScheme);
        await HttpContext.SignInAsync(CustomerScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        return RedirectToAction("Index", "Home");
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

    #region Profile

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
        if (!authResult.Succeeded)
        {
            return RedirectToAction("Login");
        }

        var userIdClaim = authResult.Principal.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        ViewData["Title"] = "Profilim";
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Profile(string firstName, string lastName, string email, string? city, DateTime? birthDate, string? phoneNumber)
    {
        var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
        if (!authResult.Succeeded)
        {
            return RedirectToAction("Login");
        }

        var userIdClaim = authResult.Principal.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // Validation
        if (string.IsNullOrWhiteSpace(firstName))
        {
            ViewBag.Error = "Ad alanı boş bırakılamaz.";
            return View(user);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            ViewBag.Error = "Soyad alanı boş bırakılamaz.";
            return View(user);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ViewBag.Error = "E-posta alanı boş bırakılamaz.";
            return View(user);
        }

        // E-posta değiştiyse, başka bir kullanıcı tarafından kullanılıp kullanılmadığını kontrol et
        if (user.Email != email.Trim().ToLower())
        {
            var existingUser = _dbContext.Users.FirstOrDefault(u => u.Email == email.Trim().ToLower() && u.Id != userId);
            if (existingUser != null)
            {
                ViewBag.Error = "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.";
                return View(user);
            }
        }

        // Telefon numarası formatını düzelt
        string? formattedPhone = null;
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            // Eğer +90 ile başlamıyorsa ekle
            if (!cleanPhone.StartsWith("+90"))
            {
                if (cleanPhone.StartsWith("0"))
                {
                    cleanPhone = "+9" + cleanPhone;
                }
                else
                {
                    cleanPhone = "+90" + cleanPhone;
                }
            }
            formattedPhone = cleanPhone;
        }

        // Güncelle
        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        user.Email = email.Trim().ToLower();
        user.City = city?.Trim();
        user.BirthDate = birthDate;
        user.PhoneNumber = formattedPhone;

        await _dbContext.SaveChangesAsync();

        // Oturumu yeniden oluştur (isim değişmiş olabilir)
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
            ExpiresUtc = DateTime.UtcNow.AddDays(30)
        };

        await HttpContext.SignOutAsync(CustomerScheme);
        await HttpContext.SignInAsync(CustomerScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        ViewBag.Success = "Profiliniz başarıyla güncellendi.";
        ViewData["Title"] = "Profilim";
        return View(user);
    }

    #endregion
}
