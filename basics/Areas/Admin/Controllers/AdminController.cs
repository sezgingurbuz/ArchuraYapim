using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using basics.Data; // Projene göre namespace'i kontrol et
using basics.Areas.Admin.Models; // Projene göre namespace'i kontrol et
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin,Editor")]
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
            var viewModel = new DashboardViewModel();
            
            // İstatistikler
            viewModel.ToplamEtkinlik = _context.Etkinlikler.Count();
            viewModel.AktifEtkinlik = _context.Etkinlikler.Count(e => e.TarihSaat >= DateTime.Now);
            viewModel.TamamlananEtkinlik = _context.EtkinlikRaporlari.Count();
            viewModel.ToplamSalon = _context.Salonlar.Count();
            viewModel.ToplamSatilanBilet = _context.EtkinlikKoltuklari.Count(k => k.DoluMu);
            viewModel.ToplamHasilat = _context.EtkinlikKoltuklari.Where(k => k.DoluMu).Sum(k => k.Fiyat);
            
            // Yaklaşan 5 etkinlik
            var yakinEtkinlikler = _context.Etkinlikler
                .Where(e => e.TarihSaat >= DateTime.Now)
                .OrderBy(e => e.TarihSaat)
                .Take(5)
                .Select(e => new YakinEtkinlik
                {
                    Id = e.Id,
                    EtkinlikAdi = e.EtkinlikAdi,
                    SalonAdi = e.Salon.SalonAdi,
                    TarihSaat = e.TarihSaat,
                    SatilanBilet = _context.EtkinlikKoltuklari.Count(k => k.EtkinlikId == e.Id && k.DoluMu),
                    ToplamKapasite = _context.EtkinlikKoltuklari.Count(k => k.EtkinlikId == e.Id)
                })
                .ToList();
            viewModel.YakinEtkinlikler = yakinEtkinlikler;
            
            // Son 10 satış
            var sonSatislar = _context.EtkinlikKoltuklari
                .Where(k => k.DoluMu)
                .OrderByDescending(k => k.SatisTarihi)
                .Take(10)
                .Select(k => new SonSatis
                {
                    EtkinlikAdi = k.Etkinlik.EtkinlikAdi,
                    KoltukNo = k.KoltukNo,
                    MusteriAdi = k.MusteriAdi + " " + k.MusteriSoyadi,
                    Fiyat = k.Fiyat,
                    SatisTarihi = k.SatisTarihi ?? DateTime.Now,
                    SatisPlatformu = k.SatisPlatformu
                })
                .ToList();
            viewModel.SonSatislar = sonSatislar;
            
            return View(viewModel);
        }

        // Kullanıcı Yönetimi Sayfası - Sadece Admin
        [Authorize(Roles = "Admin")]
        public IActionResult Users()
        {
            var users = _context.AdminUsers.OrderBy(u => u.userName).ToList();
            return View(users);
        }

        // Yeni Kullanıcı Ekle
        [HttpPost]
        public async Task<IActionResult> AddUser(string userName, string password, string role)
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
                passwordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
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
                // Admin kullanıcısını silmeye izin verme
                if (user.userName.ToLower() == "admin")
                {
                    TempData["ErrorMessage"] = "Admin kullanıcısı silinemez!";
                    return RedirectToAction("Users");
                }
                
                _context.AdminUsers.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            }
            return RedirectToAction("Users");
        }
    }
}