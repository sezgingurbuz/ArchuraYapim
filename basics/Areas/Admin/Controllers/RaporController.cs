using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using basics.Data;
using basics.Areas.Admin.Models;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin,Okan")]
    public class RaporController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RaporController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Raporları veri tabanından çek (Artık etkinliklerden değil, raporlar tablosundan)
            var raporlar = _context.EtkinlikRaporlari
                .OrderByDescending(r => r.TarihSaat)
                .ToList();

            var raporListesi = raporlar.Select(r => new RaporViewModel
            {
                EtkinlikId = r.Id,
                EtkinlikAdi = r.EtkinlikAdi,
                SalonAdi = r.SalonAdi,
                Sehir = r.Sehir,
                Tarih = r.TarihSaat,
                ToplamKapasite = r.ToplamKapasite,
                SatilanBilet = r.SatilanBilet,
                BosKoltuk = r.BosKoltuk,
                ToplamHasilat = r.ToplamHasilat,
                BubiletSatisAdedi = r.BubiletSatisAdedi,
                BiletinialSatisAdedi = r.BiletinialSatisAdedi,
                NakitSatisAdedi = r.NakitSatisAdedi,
                NakitHasilat = r.NakitHasilat,
                KartSatisAdedi = r.KartSatisAdedi,
                KartHasilat = r.KartHasilat,
                EFTSatisAdedi = r.EFTSatisAdedi,
                EFTHasilat = r.EFTHasilat
            }).ToList();

            return View(raporListesi);
        }

        [HttpPost]
        public async Task<IActionResult> RaporaGonder(int id)
        {
            var etkinlik = await _context.Etkinlikler
                .Include(e => e.Salon)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            // Etkinlik koltuk verilerini çek
            var biletler = await _context.EtkinlikKoltuklari
                .Where(k => k.EtkinlikId == id)
                .ToListAsync();

            // İstatistikleri hesapla
            int toplamKapasite = biletler.Count;
            int satilan = biletler.Count(k => k.DoluMu);
            decimal hasilat = biletler.Where(k => k.DoluMu).Sum(k => k.Fiyat);
            
            int bubiletSatis = biletler.Count(k => k.DoluMu && k.SatisPlatformu == "Bubilet");
            int biletinialSatis = biletler.Count(k => k.DoluMu && k.SatisPlatformu == "Biletinial");
            
            var nakitSatislar = biletler.Where(k => k.DoluMu && k.OdemeYontemi == "Nakit");
            var kartSatislar = biletler.Where(k => k.DoluMu && k.OdemeYontemi == "Kart");
            var eftSatislar = biletler.Where(k => k.DoluMu && k.OdemeYontemi == "EFT");

            // Rapor oluştur ve kaydet
            var rapor = new EtkinlikRapor
            {
                EtkinlikAdi = etkinlik.EtkinlikAdi,
                Tur = etkinlik.Tur,
                SalonAdi = etkinlik.Salon.SalonAdi,
                Sehir = etkinlik.Salon.Sehir,
                TarihSaat = etkinlik.TarihSaat,
                ToplamKapasite = toplamKapasite,
                SatilanBilet = satilan,
                BosKoltuk = toplamKapasite - satilan,
                ToplamHasilat = hasilat,
                BubiletSatisAdedi = bubiletSatis,
                BiletinialSatisAdedi = biletinialSatis,
                NakitSatisAdedi = nakitSatislar.Count(),
                NakitHasilat = nakitSatislar.Sum(k => k.Fiyat),
                KartSatisAdedi = kartSatislar.Count(),
                KartHasilat = kartSatislar.Sum(k => k.Fiyat),
                EFTSatisAdedi = eftSatislar.Count(),
                EFTHasilat = eftSatislar.Sum(k => k.Fiyat),
                RaporTarihi = DateTime.Now,
                RaporlayanKullanici = User.Identity.Name ?? "Bilinmiyor"
            };

            _context.EtkinlikRaporlari.Add(rapor);

            // Koltuk kayıtlarını sil (Veri tabanı yükünü azaltmak için)
            _context.EtkinlikKoltuklari.RemoveRange(biletler);
            
            // Etkinliği de sil (Artık raporda olduğu için gerek yok)
            _context.Etkinlikler.Remove(etkinlik);
            
            // Salonu kontrol et - bu salona ait başka aktif etkinlik var mı?
            var salonId = etkinlik.Salon.Id;
            var salonHasOtherEvents = await _context.Etkinlikler
                .AnyAsync(e => e.Salon.Id == salonId && e.Id != id);
            
            // Eğer salona ait başka etkinlik yoksa, salonu Pasif yap
            if (!salonHasOtherEvents)
            {
                etkinlik.Salon.Durum = "Pasif";
                _context.Salonlar.Update(etkinlik.Salon);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{etkinlik.EtkinlikAdi} etkinliği başarıyla raporlandı ve koltuk verileri temizlendi.";
            return RedirectToAction("Index", "Etkinlikler");
        }

        [HttpPost]
        public async Task<IActionResult> Sil(int id)
        {
            var rapor = await _context.EtkinlikRaporlari.FindAsync(id);
            
            if (rapor == null)
            {
                TempData["ErrorMessage"] = "Rapor bulunamadı.";
                return RedirectToAction("Index");
            }

            _context.EtkinlikRaporlari.Remove(rapor);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{rapor.EtkinlikAdi} raporu başarıyla silindi.";
            return RedirectToAction("Index");
        }
    }
}