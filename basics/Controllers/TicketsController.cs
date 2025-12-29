using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using basics.Data;
using basics.Areas.Admin.Models;
using basics.Models;
using System.Security.Claims;

namespace basics.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private const string CustomerScheme = "CustomerScheme";

        public TicketsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(string? sehir)
        {
            // Gelecek etkinlikleri çek (süresi geçmişler hariç)
            var query = _dbContext.Etkinlikler
                .Include(e => e.Salon)
                .Where(e => e.TarihSaat >= DateTime.Now);

            // Şehre göre filtrele
            if (!string.IsNullOrEmpty(sehir))
            {
                query = query.Where(e => e.Salon != null && e.Salon.Sehir == sehir);
            }

            // Tarihe göre artan sırada listele (en yakın etkinlik başta)
            var etkinlikler = query
                .OrderBy(e => e.TarihSaat)
                .ToList();

            // Şehir listesi (filtreleme için)
            var sehirler = _dbContext.Salonlar
                .Select(s => s.Sehir)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            ViewBag.Sehirler = sehirler;
            ViewBag.SecilenSehir = sehir;

            return View(etkinlikler);
        }

        [HttpGet]
        public async Task<IActionResult> Biletlerim()
        {
            // Müşteri oturumu kontrolü
            var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
            if (!authResult.Succeeded)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Tickets/Biletlerim" });
            }

            // Kullanıcı ID'sini CustomerScheme'den al
            var userIdClaim = authResult.Principal?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim);

            // Kullanıcının biletlerini çek
            var biletler = _dbContext.EtkinlikKoltuklari
                .Include(b => b.Etkinlik)
                    .ThenInclude(e => e.Salon)
                .Where(b => b.UserId == userId && b.DoluMu)
                .OrderByDescending(b => b.SatisTarihi)
                .ToList();

            // Aynı etkinlik ve satış tarihine göre grupla (aynı anda alınan biletler)
            var grupluBiletler = biletler
                .GroupBy(b => new { b.EtkinlikId, SatisTarihi = b.SatisTarihi?.ToString("yyyyMMddHHmmss") })
                .Select(g => new BiletGrubu
                {
                    Biletler = g.ToList(),
                    Etkinlik = g.First().Etkinlik,
                    SatisTarihi = g.First().SatisTarihi,
                    ToplamFiyat = g.Sum(b => b.Fiyat),
                    Koltuklar = string.Join(", ", g.Select(b => b.KoltukNo))
                })
                .OrderByDescending(g => g.SatisTarihi)
                .ToList();

            return View(grupluBiletler);
        }

        [HttpGet]
        public async Task<IActionResult> BiletAl(int id)
        {
            // Etkinliği çek
            var etkinlik = _dbContext.Etkinlikler
                .Include(e => e.Salon)
                    .ThenInclude(s => s.SeatingPlan)
                .FirstOrDefault(e => e.Id == id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            // Satış aktif değilse geri dön
            if (!etkinlik.SatisAktifMi)
            {
                TempData["ErrorMessage"] = "Bu etkinlik için bilet satışı henüz başlamamıştır.";
                return RedirectToAction("Index");
            }

            // Geçmiş etkinlik kontrolü
            if (etkinlik.TarihSaat < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Bu etkinliğin tarihi geçmiştir.";
                return RedirectToAction("Index");
            }

            // Bilet bilgilerini çek
            var biletler = _dbContext.EtkinlikKoltuklari
                .Where(b => b.EtkinlikId == id)
                .ToList();

            ViewBag.Biletler = biletler;

            // Giriş yapmış müşteri bilgileri (CustomerScheme'den)
            var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
            if (authResult.Succeeded)
            {
                var userId = authResult.Principal?.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = _dbContext.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
                    if (user != null)
                    {
                        ViewBag.KullaniciAdi = user.FirstName;
                        ViewBag.KullaniciSoyadi = user.LastName;
                        ViewBag.KullaniciEmail = user.Email;
                        ViewBag.KullaniciTelefon = user.PhoneNumber;
                    }
                }
            }

            return View(etkinlik);
        }

        [HttpPost]
        public async Task<IActionResult> BiletAl(int etkinlikId, string koltuklar, decimal fiyat, 
            string musteriAdi, string musteriSoyadi, string musteriTelefon, string musteriEmail)
        {
            // Validasyon
            if (string.IsNullOrWhiteSpace(koltuklar))
            {
                TempData["ErrorMessage"] = "Lütfen en az bir koltuk seçin.";
                return RedirectToAction("BiletAl", new { id = etkinlikId });
            }

            if (string.IsNullOrWhiteSpace(musteriAdi) || string.IsNullOrWhiteSpace(musteriSoyadi))
            {
                TempData["ErrorMessage"] = "Ad ve soyad gereklidir.";
                return RedirectToAction("BiletAl", new { id = etkinlikId });
            }

            if (string.IsNullOrWhiteSpace(musteriTelefon))
            {
                TempData["ErrorMessage"] = "Telefon numarası gereklidir.";
                return RedirectToAction("BiletAl", new { id = etkinlikId });
            }

            // Etkinlik kontrolü
            var etkinlik = _dbContext.Etkinlikler.FirstOrDefault(e => e.Id == etkinlikId);
            if (etkinlik == null || !etkinlik.SatisAktifMi)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı veya satışa kapalı.";
                return RedirectToAction("Index");
            }

            // Seçilen koltukları ayır
            var koltukListesi = koltuklar.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .ToList();

            // User ID (müşteri giriş yapmışsa - CustomerScheme'den)
            int? userId = null;
            var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
            if (authResult.Succeeded)
            {
                var userIdClaim = authResult.Principal?.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    userId = int.Parse(userIdClaim);
                }
            }

            // Her koltuk için satış yap
            foreach (var koltukNo in koltukListesi)
            {
                var bilet = _dbContext.EtkinlikKoltuklari
                    .FirstOrDefault(b => b.EtkinlikId == etkinlikId && b.KoltukNo == koltukNo);

                if (bilet == null)
                {
                    TempData["ErrorMessage"] = $"Koltuk bulunamadı: {koltukNo}";
                    return RedirectToAction("BiletAl", new { id = etkinlikId });
                }

                if (bilet.DoluMu)
                {
                    TempData["ErrorMessage"] = $"Bu koltuk zaten satılmış: {koltukNo}";
                    return RedirectToAction("BiletAl", new { id = etkinlikId });
                }

                // Satışı kaydet
                bilet.DoluMu = true;
                bilet.UserId = userId;
                bilet.Fiyat = fiyat;
                bilet.MusteriAdi = musteriAdi.Trim();
                bilet.MusteriSoyadi = musteriSoyadi.Trim();
                bilet.MusteriTelefon = musteriTelefon.Trim();
                bilet.MusteriEmail = musteriEmail?.Trim();
                bilet.OdemeYontemi = "Online";
                bilet.SatisTarihi = DateTime.Now;
                bilet.SatisPlatformu = "Archura";
            }

            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"{koltukListesi.Count} bilet başarıyla satın alındı!";
            return RedirectToAction("Index", "Home");
        }
    }
}
