using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using basics.Data; 
using basics.Areas.Admin.Models; 
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin,Editor")]
    public class SatislarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SatislarController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(int page = 1)
        {
            const int pageSize = 12; // Kart görünümü olduğu için 12 (3x4 grid için)
            
            var query = _context.Etkinlikler
                .Include(e => e.Salon)
                .OrderByDescending(e => e.TarihSaat);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Etkinlikleri salon bilgisiyle çek
            var etkinlikler = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            // Şehir listesi
            var sehirler = _context.Salonlar
                .Select(s => s.Sehir)
                .Distinct()
                .ToList();
            
            ViewBag.Sehirler = sehirler;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            
            return View(etkinlikler);
        }

        // SATIŞ YAP (GET)
        [HttpGet]
        public async Task<IActionResult> SatisYap(int id)
        {
            var etkinlik = await _context.Etkinlikler
                .Include(e => e.Salon)
                .ThenInclude(s => s.SeatingPlan)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            // Etkinliğe ait koltukları çek
            var biletler = await _context.EtkinlikKoltuklari
                .Where(k => k.EtkinlikId == id)
                .ToListAsync();

            ViewBag.Biletler = biletler;
            
            return View(etkinlik);
        }

        // SATIŞ YAP (POST)
        [HttpPost]
        public async Task<IActionResult> SatisYap(int etkinlikId, string koltuklar, decimal fiyat, 
            string musteriAdi, string musteriSoyadi, string musteriTelefon, string? musteriEmail, string odemeYontemi, string satisPlatformu)
        {
            if (string.IsNullOrEmpty(koltuklar))
            {
                TempData["ErrorMessage"] = "Lütfen en az bir koltuk seçiniz.";
                return RedirectToAction("SatisYap", new { id = etkinlikId });
            }

            // Koltuk numaralarını ayır
            var koltukNoList = koltuklar.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .ToList();

            // Seçilen koltukları veritabanından çek
            var secilenKoltuklar = await _context.EtkinlikKoltuklari
                .Where(k => k.EtkinlikId == etkinlikId && koltukNoList.Contains(k.KoltukNo))
                .ToListAsync();

            // Dolu koltuk kontrolü
            var doluKoltuklar = secilenKoltuklar.Where(k => k.DoluMu).ToList();
            if (doluKoltuklar.Any())
            {
                TempData["ErrorMessage"] = $"Seçtiğiniz koltuklar dolu: {string.Join(", ", doluKoltuklar.Select(k => k.KoltukNo))}";
                return RedirectToAction("SatisYap", new { id = etkinlikId });
            }

            // Satış işlemini gerçekleştir
            foreach (var koltuk in secilenKoltuklar)
            {
                koltuk.DoluMu = true;
                koltuk.Fiyat = fiyat;
                koltuk.MusteriAdi = musteriAdi;
                koltuk.MusteriSoyadi = musteriSoyadi;
                koltuk.MusteriTelefon = musteriTelefon;
                koltuk.MusteriEmail = musteriEmail;
                koltuk.OdemeYontemi = odemeYontemi;
                koltuk.SatisTarihi = DateTime.Now;
                koltuk.SatisYapanKullanici = User.Identity.Name ?? "Bilinmiyor";
                koltuk.SatisPlatformu = satisPlatformu; // Archura, Bubilet, Biletinial
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{secilenKoltuklar.Count} adet bilet başarıyla satıldı.";
            return RedirectToAction("SatisYap", new { id = etkinlikId });
        }
    }
}