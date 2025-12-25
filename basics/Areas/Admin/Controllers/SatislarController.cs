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
    [Authorize]
    public class SatislarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SatislarController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Etkinlikleri salon bilgisiyle çek
            var etkinlikler = _context.Etkinlikler
                .Include(e => e.Salon)
                .OrderByDescending(e => e.TarihSaat)
                .ToList();
            
            // Şehir listesi
            var sehirler = _context.Salonlar
                .Select(s => s.Sehir)
                .Distinct()
                .ToList();
            
            ViewBag.Sehirler = sehirler;
            
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
                koltuk.SatisYapanKullanici = "Admin"; // TODO: Login sistemi eklendikten sonra gerçek kullanıcı adı kullanılacak
                koltuk.SatisPlatformu = satisPlatformu; // Archura, Bubilet, Biletinial
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{secilenKoltuklar.Count} adet bilet başarıyla satıldı.";
            return RedirectToAction("SatisYap", new { id = etkinlikId });
        }
    }
}