using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using basics.Data; // Projene göre namespace'i kontrol et
using basics.Areas.Admin.Models; // Projene göre namespace'i kontrol et
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // URL: /Admin veya /Admin/Admin/Index
        public IActionResult Index()
        {
            return View();
        }

        // Kullanıcı Yönetimi Sayfası
        public IActionResult Users()
        {
            var users = _context.AdminUsers.OrderBy(u => u.userName).ToList();
            return View(users);
        }

        // Yeni Kullanıcı Ekle
        [HttpPost]
        public async Task<IActionResult> AddUser(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Kullanıcı adı ve şifre boş olamaz!";
                return RedirectToAction("Users");
            }

            // Aynı kullanıcı adı var mı kontrol et
            if (_context.AdminUsers.Any(u => u.userName == userName))
            {
                TempData["ErrorMessage"] = "Bu kullanıcı adı zaten mevcut!";
                return RedirectToAction("Users");
            }

            var newUser = new AdminUser
            {
                userName = userName,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.AdminUsers.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni kullanıcı başarıyla eklendi.";
            return RedirectToAction("Users");
        }

        // Kullanıcı Sil
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user != null)
            {
                _context.AdminUsers.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            }
            return RedirectToAction("Users");
        }
    }
}