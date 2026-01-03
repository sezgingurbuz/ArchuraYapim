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
                .Include(e => e.Kategoriler)
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

            // Kategori satışı ise, atanmamış kategori bilet sayısını hesapla
            if (etkinlik.SatisTipi == "Kategori")
            {
                var kategoriIds = etkinlik.Kategoriler?.Select(k => k.Id).ToList() ?? new List<int>();
                var toplam = await _context.KategoriBiletler
                    .Where(kb => kategoriIds.Contains(kb.EtkinlikKategoriId))
                    .CountAsync();
                var atanmamis = await _context.KategoriBiletler
                    .Where(kb => kategoriIds.Contains(kb.EtkinlikKategoriId) && !kb.KoltukAtandiMi)
                    .CountAsync();
                
                ViewBag.ToplamKategoriBilet = toplam;
                ViewBag.AtanmamisKategoriBilet = atanmamis;
            }
            
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

        // REZERVASYON KODU İLE BİLET ARA (GET)
        [HttpGet]
        public IActionResult RezervasyonAra(int etkinlikId)
        {
            ViewBag.EtkinlikId = etkinlikId;
            return View();
        }

        // REZERVASYON KODU İLE BİLET ARA (POST) - AJAX
        [HttpPost]
        public async Task<IActionResult> RezervasyonBul(string rezervasyonKodu)
        {
            if (string.IsNullOrWhiteSpace(rezervasyonKodu))
            {
                return Json(new { success = false, message = "Rezervasyon kodu boş olamaz." });
            }

            // Rezervasyon kodunu ara
            var kategoriBilet = await _context.KategoriBiletler
                .Include(kb => kb.EtkinlikKategori)
                    .ThenInclude(ek => ek.Etkinlik)
                        .ThenInclude(e => e.Salon)
                .Include(kb => kb.AtananKoltuk)
                .FirstOrDefaultAsync(kb => kb.RezervasyonKodu == rezervasyonKodu.Trim().ToUpper());

            if (kategoriBilet == null)
            {
                return Json(new { success = false, message = "Bu rezervasyon kodu bulunamadı." });
            }

            if (kategoriBilet.KoltukAtandiMi)
            {
                return Json(new { 
                    success = false, 
                    message = $"Bu bilete zaten koltuk atanmış: {kategoriBilet.AtananKoltuk?.KoltukNo}" 
                });
            }

            return Json(new { 
                success = true,
                bilet = new {
                    id = kategoriBilet.Id,
                    rezervasyonKodu = kategoriBilet.RezervasyonKodu,
                    musteriAdi = kategoriBilet.MusteriAdi,
                    musteriSoyadi = kategoriBilet.MusteriSoyadi,
                    musteriTelefon = kategoriBilet.MusteriTelefon,
                    musteriEmail = kategoriBilet.MusteriEmail,
                    kategoriAdi = kategoriBilet.EtkinlikKategori?.KategoriAdi,
                    fiyat = kategoriBilet.OdenenFiyat,
                    etkinlikId = kategoriBilet.EtkinlikKategori?.EtkinlikId,
                    etkinlikAdi = kategoriBilet.EtkinlikKategori?.Etkinlik?.EtkinlikAdi,
                    salonAdi = kategoriBilet.EtkinlikKategori?.Etkinlik?.Salon?.SalonAdi
                }
            });
        }

        // KOLTUK ATAMA (GET) - Kategori bileti için koltuk düzeni göster
        [HttpGet]
        public async Task<IActionResult> KoltukAtama(int biletId)
        {
            var kategoriBilet = await _context.KategoriBiletler
                .Include(kb => kb.EtkinlikKategori)
                    .ThenInclude(ek => ek.Etkinlik)
                        .ThenInclude(e => e.Salon)
                            .ThenInclude(s => s.SeatingPlan)
                .FirstOrDefaultAsync(kb => kb.Id == biletId);

            if (kategoriBilet == null)
            {
                TempData["ErrorMessage"] = "Bilet bulunamadı.";
                return RedirectToAction("Index");
            }

            if (kategoriBilet.KoltukAtandiMi)
            {
                TempData["ErrorMessage"] = "Bu bilete zaten koltuk atanmış.";
                return RedirectToAction("Index");
            }

            // Etkinliğe ait koltukları çek
            var etkinlikId = kategoriBilet.EtkinlikKategori.EtkinlikId;
            var biletler = await _context.EtkinlikKoltuklari
                .Where(k => k.EtkinlikId == etkinlikId)
                .ToListAsync();

            ViewBag.Biletler = biletler;
            ViewBag.KategoriBilet = kategoriBilet;
            ViewBag.Etkinlik = kategoriBilet.EtkinlikKategori.Etkinlik;

            return View();
        }

        // KOLTUK ATAMA (POST)
        [HttpPost]
        public async Task<IActionResult> KoltukAtama(int biletId, string koltukNo)
        {
            var kategoriBilet = await _context.KategoriBiletler
                .Include(kb => kb.EtkinlikKategori)
                .FirstOrDefaultAsync(kb => kb.Id == biletId);

            if (kategoriBilet == null)
            {
                TempData["ErrorMessage"] = "Bilet bulunamadı.";
                return RedirectToAction("Index");
            }

            if (kategoriBilet.KoltukAtandiMi)
            {
                TempData["ErrorMessage"] = "Bu bilete zaten koltuk atanmış.";
                return RedirectToAction("Index");
            }

            // Seçilen koltuğu bul
            var etkinlikId = kategoriBilet.EtkinlikKategori.EtkinlikId;
            var koltuk = await _context.EtkinlikKoltuklari
                .FirstOrDefaultAsync(k => k.EtkinlikId == etkinlikId && k.KoltukNo == koltukNo);

            if (koltuk == null)
            {
                TempData["ErrorMessage"] = "Koltuk bulunamadı.";
                return RedirectToAction("KoltukAtama", new { biletId });
            }

            if (koltuk.DoluMu)
            {
                TempData["ErrorMessage"] = "Bu koltuk zaten dolu.";
                return RedirectToAction("KoltukAtama", new { biletId });
            }

            // Koltuk ataması yap
            kategoriBilet.AtananKoltukId = koltuk.Id;
            kategoriBilet.KoltukAtandiMi = true;
            kategoriBilet.KoltukAtamaTarihi = DateTime.Now;

            // Koltuğu da dolu olarak işaretle
            koltuk.DoluMu = true;
            koltuk.MusteriAdi = kategoriBilet.MusteriAdi;
            koltuk.MusteriSoyadi = kategoriBilet.MusteriSoyadi;
            koltuk.MusteriTelefon = kategoriBilet.MusteriTelefon;
            koltuk.MusteriEmail = kategoriBilet.MusteriEmail;
            koltuk.Fiyat = kategoriBilet.OdenenFiyat;
            koltuk.OdemeYontemi = kategoriBilet.OdemeYontemi;
            koltuk.SatisTarihi = kategoriBilet.SatisTarihi;
            koltuk.SatisPlatformu = "Archura (Kategori)";
            koltuk.BiletKodu = kategoriBilet.BiletKodu; // QR kod için aynı kodu kullan
            koltuk.SatisYapanKullanici = User.Identity?.Name ?? "Gişe";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Koltuk başarıyla atandı: {koltukNo}. Rezervasyon Kodu: {kategoriBilet.RezervasyonKodu}";
            
            // QR kod bilgisini göster
            ViewBag.BiletKodu = kategoriBilet.BiletKodu;
            
            return RedirectToAction("SatisYap", new { id = etkinlikId });
        }
    }
}