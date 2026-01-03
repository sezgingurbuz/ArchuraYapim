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

            // Kullanıcının KOLTUK biletlerini çek
            var koltukBiletler = _dbContext.EtkinlikKoltuklari
                .Include(b => b.Etkinlik)
                    .ThenInclude(e => e.Salon)
                .Where(b => b.UserId == userId && b.DoluMu)
                .OrderByDescending(b => b.SatisTarihi)
                .ToList();

            // Aynı etkinlik ve satış tarihine göre grupla (aynı anda alınan biletler)
            var grupluBiletler = koltukBiletler
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

            // Kullanıcının KATEGORİ biletlerini çek
            var kategoriBiletler = await _dbContext.KategoriBiletler
                .Include(kb => kb.EtkinlikKategori)
                    .ThenInclude(ek => ek.Etkinlik)
                        .ThenInclude(e => e.Salon)
                .Include(kb => kb.AtananKoltuk)
                .Where(kb => kb.UserId == userId)
                .OrderByDescending(kb => kb.SatisTarihi)
                .ToListAsync();

            ViewBag.KategoriBiletler = kategoriBiletler;

            return View(grupluBiletler);
        }

        [HttpGet]
        public async Task<IActionResult> KategoriBiletDetay(int biletId)
        {
            // Müşteri oturumu kontrolü
            var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
            if (!authResult.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = authResult.Principal?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim);

            // Bileti çek ve kullanıcıya ait olduğunu doğrula
            var bilet = await _dbContext.KategoriBiletler
                .Include(kb => kb.EtkinlikKategori)
                    .ThenInclude(ek => ek.Etkinlik)
                        .ThenInclude(e => e.Salon)
                .Include(kb => kb.AtananKoltuk)
                .FirstOrDefaultAsync(kb => kb.Id == biletId && kb.UserId == userId);

            if (bilet == null)
            {
                TempData["ErrorMessage"] = "Bilet bulunamadı.";
                return RedirectToAction("Biletlerim");
            }

            if (!bilet.KoltukAtandiMi)
            {
                TempData["ErrorMessage"] = "Bu bilete henüz koltuk atanmamış.";
                return RedirectToAction("Biletlerim");
            }

            return View(bilet);
        }

        [HttpGet]
        public async Task<IActionResult> BiletAl(int id)
        {
            // Etkinliği çek (kategorileri de dahil et)
            var etkinlik = _dbContext.Etkinlikler
                .Include(e => e.Salon)
                    .ThenInclude(s => s.SeatingPlan)
                .Include(e => e.Kategoriler)
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

            // Satış tipine göre veri hazırla
            ViewBag.SatisTipi = etkinlik.SatisTipi;

            if (etkinlik.SatisTipi == "Kategori")
            {
                // Kategori satışı için kategorileri gönder
                ViewBag.Kategoriler = etkinlik.Kategoriler?.ToList() ?? new List<EtkinlikKategori>();
                
                // Her kategori için satılan bilet sayısını hesapla
                var kategoriSatisSayilari = new Dictionary<int, int>();
                foreach (var kat in etkinlik.Kategoriler ?? Enumerable.Empty<EtkinlikKategori>())
                {
                    var satilanAdet = _dbContext.KategoriBiletler.Count(kb => kb.EtkinlikKategoriId == kat.Id);
                    kategoriSatisSayilari[kat.Id] = satilanAdet;
                }
                ViewBag.KategoriSatisSayilari = kategoriSatisSayilari;
            }
            else
            {
                // Koltuk satışı için biletleri gönder
                var biletler = _dbContext.EtkinlikKoltuklari
                    .Where(b => b.EtkinlikId == id)
                    .ToList();
                ViewBag.Biletler = biletler;
            }

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

        [HttpGet]
        public async Task<IActionResult> OdemeYap(int etkinlikId, string koltuklar, decimal toplamFiyat)
        {
            // Validasyon
            if (string.IsNullOrWhiteSpace(koltuklar))
            {
                TempData["ErrorMessage"] = "Lütfen koltuk seçin.";
                return RedirectToAction("BiletAl", new { id = etkinlikId });
            }

            // Etkinlik bilgilerini çek
            var etkinlik = _dbContext.Etkinlikler
                .Include(e => e.Salon)
                .FirstOrDefault(e => e.Id == etkinlikId);

            if (etkinlik == null)
            {
                return NotFound();
            }

            // Seçilen koltukları ayır
            var koltukListesi = koltuklar.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .ToList();

            // koltukların hala müsait olduğunu kontrol et
            var satilmisKoltuklar = _dbContext.EtkinlikKoltuklari
                .Where(b => b.EtkinlikId == etkinlikId && koltukListesi.Contains(b.KoltukNo) && b.DoluMu)
                .Select(b => b.KoltukNo)
                .ToList();

            if (satilmisKoltuklar.Any())
            {
                TempData["ErrorMessage"] = $"Seçtiğiniz bazı koltuklar artık müsait değil: {string.Join(", ", satilmisKoltuklar)}";
                return RedirectToAction("BiletAl", new { id = etkinlikId });
            }

            // Giriş yapmış müşteri bilgileri
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
                        ViewBag.GirisYapildi = true;
                    }
                }
            }

            ViewBag.Etkinlik = etkinlik;
            ViewBag.Koltuklar = koltukListesi;
            ViewBag.ToplamFiyat = toplamFiyat;
            ViewBag.BiletFiyati = koltukListesi.Count > 0 ? toplamFiyat / koltukListesi.Count : 0;

            return View();
        }

        // KATEGORİ SATIŞ - ÖDEME SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> KategoriOdeme(int kategoriId, int adet = 1)
        {
            // Kategoriyi çek
            var kategori = await _dbContext.EtkinlikKategorileri
                .Include(k => k.Etkinlik)
                    .ThenInclude(e => e.Salon)
                .FirstOrDefaultAsync(k => k.Id == kategoriId);

            if (kategori == null)
            {
                TempData["ErrorMessage"] = "Kategori bulunamadı.";
                return RedirectToAction("Index");
            }

            // Etkinlik kontrolü
            if (!kategori.Etkinlik.SatisAktifMi)
            {
                TempData["ErrorMessage"] = "Bu etkinlik için bilet satışı henüz başlamamıştır.";
                return RedirectToAction("Index");
            }

            // Kontenjan kontrolü
            if (kategori.Kontenjan.HasValue)
            {
                var satilanAdet = _dbContext.KategoriBiletler.Count(kb => kb.EtkinlikKategoriId == kategoriId);
                var kalanKontenjan = kategori.Kontenjan.Value - satilanAdet;
                
                if (adet > kalanKontenjan)
                {
                    TempData["ErrorMessage"] = $"Bu kategoride yeterli kontenjan kalmadı. Kalan: {kalanKontenjan}";
                    return RedirectToAction("BiletAl", new { id = kategori.EtkinlikId });
                }
            }

            // Giriş yapmış müşteri bilgileri
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
                        ViewBag.GirisYapildi = true;
                    }
                }
            }

            ViewBag.Kategori = kategori;
            ViewBag.Etkinlik = kategori.Etkinlik;
            ViewBag.Adet = adet;
            ViewBag.ToplamFiyat = kategori.Fiyat * adet;

            return View();
        }

        // KATEGORİ SATIŞ - ÖDEME İŞLEMİ (POST)
        [HttpPost]
        public async Task<IActionResult> KategoriOdeme(int kategoriId, int adet,
            string musteriAdi, string musteriSoyadi, string musteriTelefon, string musteriEmail,
            string kartNumarasi, string sonKullanma, string cvv, string kartSahibi)
        {
            // BAKIM MODU KONTROLÜ
            if (true) // Bakım modu aktif
            {
                TempData["ErrorMessage"] = "Ödeme sistemi bakım çalışması nedeniyle geçici olarak devre dışıdır. Lütfen gişemizle iletişime geçiniz.";
                return RedirectToAction("Index", "Home");
            }
            // Kategoriyi çek
            var kategori = await _dbContext.EtkinlikKategorileri
                .Include(k => k.Etkinlik)
                .FirstOrDefaultAsync(k => k.Id == kategoriId);

            if (kategori == null)
            {
                TempData["ErrorMessage"] = "Kategori bulunamadı.";
                return RedirectToAction("Index");
            }

            // Validasyon
            if (string.IsNullOrWhiteSpace(musteriAdi) || string.IsNullOrWhiteSpace(musteriSoyadi))
            {
                TempData["ErrorMessage"] = "Ad ve soyad gereklidir.";
                return RedirectToAction("KategoriOdeme", new { kategoriId, adet });
            }

            if (string.IsNullOrWhiteSpace(musteriTelefon))
            {
                TempData["ErrorMessage"] = "Telefon numarası gereklidir.";
                return RedirectToAction("KategoriOdeme", new { kategoriId, adet });
            }

            // Kart validasyonu
            if (string.IsNullOrWhiteSpace(kartNumarasi) || kartNumarasi.Replace(" ", "").Length < 16)
            {
                TempData["ErrorMessage"] = "Geçerli bir kart numarası girin.";
                return RedirectToAction("KategoriOdeme", new { kategoriId, adet });
            }

            // Kontenjan kontrolü
            if (kategori.Kontenjan.HasValue)
            {
                var satilanAdet = _dbContext.KategoriBiletler.Count(kb => kb.EtkinlikKategoriId == kategoriId);
                var kalanKontenjan = kategori.Kontenjan.Value - satilanAdet;
                
                if (adet > kalanKontenjan)
                {
                    TempData["ErrorMessage"] = $"Bu kategoride yeterli kontenjan kalmadı. Kalan: {kalanKontenjan}";
                    return RedirectToAction("BiletAl", new { id = kategori.EtkinlikId });
                }
            }

            // User ID (müşteri giriş yapmışsa)
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

            // Her bilet için KategoriBilet oluştur
            var biletler = new List<KategoriBilet>();
            for (int i = 0; i < adet; i++)
            {
                // Benzersiz rezervasyon kodu oluştur: RZV-XXXXXX
                var rezervasyonKodu = $"RZV-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                
                var yeniBilet = new KategoriBilet
                {
                    EtkinlikKategoriId = kategoriId,
                    RezervasyonKodu = rezervasyonKodu,
                    UserId = userId,
                    MusteriAdi = musteriAdi.Trim(),
                    MusteriSoyadi = musteriSoyadi.Trim(),
                    MusteriTelefon = musteriTelefon.Trim(),
                    MusteriEmail = musteriEmail?.Trim(),
                    SatisTarihi = DateTime.Now,
                    OdemeYontemi = "Kredi Kartı",
                    OdenenFiyat = kategori.Fiyat,
                    KoltukAtandiMi = false,
                    BiletKodu = Guid.NewGuid()
                };
                biletler.Add(yeniBilet);
            }

            await _dbContext.KategoriBiletler.AddRangeAsync(biletler);
            await _dbContext.SaveChangesAsync();

            // Satın alınan rezervasyon kodlarını göster
            var kodlar = string.Join(", ", biletler.Select(b => b.RezervasyonKodu));
            TempData["SuccessMessage"] = $"{adet} adet bilet başarıyla satın alındı! Rezervasyon kodlarınız: {kodlar}";
            TempData["RezervasyonKodlari"] = biletler.Select(b => b.RezervasyonKodu).ToList();

            // Giriş yapmışsa biletlerim sayfasına, yapmamışsa ana sayfaya yönlendir
            if (userId.HasValue)
            {
                return RedirectToAction("Biletlerim");
            }
            
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> OdemeYap(int etkinlikId, string koltuklar, decimal fiyat,
            string musteriAdi, string musteriSoyadi, string musteriTelefon, string musteriEmail,
            string kartNumarasi, string sonKullanma, string cvv, string kartSahibi)
        {
            // BAKIM MODU KONTROLÜ
            if (true) // Bakım modu aktif
            {
                TempData["ErrorMessage"] = "Ödeme sistemi bakım çalışması nedeniyle geçici olarak devre dışıdır. Lütfen gişemizle iletişime geçiniz.";
                return RedirectToAction("Index", "Home");
            }
            // Validasyon
            if (string.IsNullOrWhiteSpace(koltuklar))
            {
                TempData["ErrorMessage"] = "Lütfen en az bir koltuk seçin.";
                return RedirectToAction("BiletAl", new { id = etkinlikId });
            }

            if (string.IsNullOrWhiteSpace(musteriAdi) || string.IsNullOrWhiteSpace(musteriSoyadi))
            {
                TempData["ErrorMessage"] = "Ad ve soyad gereklidir.";
                return RedirectToAction("OdemeYap", new { etkinlikId, koltuklar, toplamFiyat = fiyat });
            }

            if (string.IsNullOrWhiteSpace(musteriTelefon))
            {
                TempData["ErrorMessage"] = "Telefon numarası gereklidir.";
                return RedirectToAction("OdemeYap", new { etkinlikId, koltuklar, toplamFiyat = fiyat });
            }

            // Kart validasyonu (basit format kontrolü)
            if (string.IsNullOrWhiteSpace(kartNumarasi) || kartNumarasi.Replace(" ", "").Length < 16)
            {
                TempData["ErrorMessage"] = "Geçerli bir kart numarası girin.";
                return RedirectToAction("OdemeYap", new { etkinlikId, koltuklar, toplamFiyat = fiyat });
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

            // Bilet fiyatını hesapla
            var biletFiyati = koltukListesi.Count > 0 ? fiyat / koltukListesi.Count : 0;

            // User ID (müşteri giriş yapmışsa)
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
                bilet.Fiyat = biletFiyati;
                bilet.MusteriAdi = musteriAdi.Trim();
                bilet.MusteriSoyadi = musteriSoyadi.Trim();
                bilet.MusteriTelefon = musteriTelefon.Trim();
                bilet.MusteriEmail = musteriEmail?.Trim();
                bilet.OdemeYontemi = "Kredi Kartı";
                bilet.SatisTarihi = DateTime.Now;
                bilet.SatisPlatformu = "Archura";
            }

            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"{koltukListesi.Count} bilet başarıyla satın alındı! Biletleriniz e-posta adresinize gönderilecektir.";
            
            // Giriş yapmışsa biletlerim sayfasına, yapmamışsa ana sayfaya yönlendir
            if (userId.HasValue)
            {
                return RedirectToAction("Biletlerim");
            }
            
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> BiletAl(int etkinlikId, string koltuklar, decimal fiyat, 
            string musteriAdi, string musteriSoyadi, string musteriTelefon, string musteriEmail)
        {
            // BAKIM MODU KONTROLÜ
            if (true) // Bakım modu aktif
            {
                TempData["ErrorMessage"] = "Online işlem sistemi geçici olarak devre dışıdır. Lütfen gişemizle iletişime geçiniz.";
                return RedirectToAction("Index", "Home");
            }
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

        /// <summary>
        /// Tek bir biletin QR kodunu ve detaylarını gösterir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BiletDetay(int id)
        {
            // Müşteri oturumu kontrolü
            var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
            if (!authResult.Succeeded)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Tickets/BiletDetay/{id}" });
            }

            // Kullanıcı ID'sini CustomerScheme'den al
            var userIdClaim = authResult.Principal?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim);

            // Bileti bul (etkinlik ve salon dahil)
            var bilet = await _dbContext.EtkinlikKoltuklari
                .Include(b => b.Etkinlik)
                    .ThenInclude(e => e.Salon)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.DoluMu);

            if (bilet == null)
            {
                TempData["ErrorMessage"] = "Bilet bulunamadı veya size ait değil.";
                return RedirectToAction("Biletlerim");
            }

            // QR kod oluştur
            ViewBag.QrCodeDataUri = Helpers.QrCodeHelper.GenerateQrCodeDataUri(bilet.BiletKodu);

            return View(bilet);
        }

        /// <summary>
        /// Aynı anda alınmış biletlerin tamamını (birden fazla QR) gösterir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BiletGrubuDetay(int etkinlikId, string satisTarihi)
        {
            // Müşteri oturumu kontrolü
            var authResult = await HttpContext.AuthenticateAsync(CustomerScheme);
            if (!authResult.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcı ID'sini CustomerScheme'den al
            var userIdClaim = authResult.Principal?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim);

            // Aynı satış tarihinde alınan biletleri bul
            var biletler = await _dbContext.EtkinlikKoltuklari
                .Include(b => b.Etkinlik)
                    .ThenInclude(e => e.Salon)
                .Where(b => b.EtkinlikId == etkinlikId && 
                            b.UserId == userId && 
                            b.DoluMu &&
                            b.SatisTarihi.HasValue)
                .ToListAsync();

            // Satış tarihine göre filtrele
            if (!string.IsNullOrEmpty(satisTarihi))
            {
                biletler = biletler
                    .Where(b => b.SatisTarihi?.ToString("yyyyMMddHHmmss") == satisTarihi)
                    .ToList();
            }

            if (!biletler.Any())
            {
                TempData["ErrorMessage"] = "Biletler bulunamadı.";
                return RedirectToAction("Biletlerim");
            }

            // Her bilet için QR kod oluştur
            var biletlerWithQr = biletler.Select(b => new BiletQrViewModel
            {
                Bilet = b,
                QrCodeDataUri = Helpers.QrCodeHelper.GenerateQrCodeDataUri(b.BiletKodu)
            }).ToList();

            return View(biletlerWithQr);
        }
    }

    // ViewModel for ticket with QR
    public class BiletQrViewModel
    {
        public EtkinlikKoltuk Bilet { get; set; } = null!;
        public string QrCodeDataUri { get; set; } = string.Empty;
    }
}
