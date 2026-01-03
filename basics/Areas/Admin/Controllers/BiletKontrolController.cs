using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using basics.Data;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class BiletKontrolController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public BiletKontrolController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// QR kod okuyucu sayfası
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// QR kod ile bilet doğrulama API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> KontrolEt([FromBody] BiletKontrolRequest request)
        {
            if (string.IsNullOrEmpty(request?.Kod))
            {
                return Json(new { success = false, message = "Bilet kodu boş olamaz!" });
            }

            // GUID'e çevir
            if (!Guid.TryParse(request.Kod, out Guid biletKodu))
            {
                return Json(new { success = false, message = "Geçersiz bilet kodu formatı!" });
            }

            // Veritabanında ara
            var bilet = await _dbContext.EtkinlikKoltuklari
                .Include(e => e.Etkinlik)
                .FirstOrDefaultAsync(e => e.BiletKodu == biletKodu);

            if (bilet == null)
            {
                return Json(new { success = false, message = "Bilet bulunamadı!" });
            }

            // Bilet satılmış mı kontrol et
            if (!bilet.DoluMu)
            {
                return Json(new { success = false, message = "Bu koltuk satılmamış!" });
            }

            // Daha önce giriş yapılmış mı?
            if (bilet.GirisYapildiMi)
            {
                return Json(new { 
                    success = false, 
                    message = "Bu bilet daha önce kullanılmış!", 
                    kullanilmis = true,
                    musteriAdi = $"{bilet.MusteriAdi} {bilet.MusteriSoyadi}",
                    koltukNo = bilet.KoltukNo,
                    etkinlikAdi = bilet.Etkinlik?.EtkinlikAdi ?? "Bilinmiyor"
                });
            }

            // Girişi işaretle
            bilet.GirisYapildiMi = true;
            await _dbContext.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "Giriş başarılı!",
                musteriAdi = $"{bilet.MusteriAdi} {bilet.MusteriSoyadi}",
                koltukNo = bilet.KoltukNo,
                etkinlikAdi = bilet.Etkinlik?.EtkinlikAdi ?? "Bilinmiyor",
                etkinlikTarihi = bilet.Etkinlik?.TarihSaat.ToString("dd.MM.yyyy HH:mm") ?? ""
            });
        }

        /// <summary>
        /// Mevcut satılmış biletlere benzersiz BiletKodu atar (tek seferlik)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateBiletKodlari()
        {
            var emptyGuid = Guid.Empty;
            var biletler = await _dbContext.EtkinlikKoltuklari
                .Where(b => b.DoluMu && b.BiletKodu == emptyGuid)
                .ToListAsync();

            if (!biletler.Any())
            {
                return Content($"Güncellenecek bilet bulunamadı. Tüm biletlerin BiletKodu zaten atanmış.");
            }

            foreach (var bilet in biletler)
            {
                bilet.BiletKodu = Guid.NewGuid();
            }

            await _dbContext.SaveChangesAsync();

            return Content($"{biletler.Count} bilete benzersiz BiletKodu atandı.");
        }
    }

    public class BiletKontrolRequest
    {
        public string? Kod { get; set; }
    }
}
