using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using basics.Data;
using basics.Areas.Admin.Models;
using System.Text.Json; // JSON işlemleri için
using Microsoft.AspNetCore.Authorization;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class EtkinliklerController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public EtkinliklerController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // SAYFAYI GETİR (GET)
        [HttpGet]
        public IActionResult Index()
        {
            var model = new EtkinlikEkleViewModel
            {
                // Şehirleri Salonlar tablosundan çekiyoruz (Distinct: Tekrarsız)
                SehirlerListesi = _dbContext.Salonlar
                    .Select(s => s.Sehir)
                    .Distinct()
                    .Select(s => new SelectListItem(s, s))
                    .ToList(),
                
                Tarih = DateTime.Today,
                
                // Mevcut etkinlikleri getir (Salon bilgisiyle beraber)
                MevcutEtkinlikler = _dbContext.Etkinlikler
                    .Include(e => e.Salon)
                    .OrderByDescending(e => e.TarihSaat)
                    .ToList()
            };

            return View(model);
        }

        // ETKİNLİĞİ KAYDET VE BİLETLERİ OLUŞTUR (POST)
        [HttpPost]
        public async Task<IActionResult> Index(EtkinlikEkleViewModel model)
        {
            // Dropdown listesinin validation'ı bozmasını engelle
            ModelState.Remove("SehirlerListesi");

            if (ModelState.IsValid)
            {
                // 1. ADIM: Etkinliği Oluştur ve Kaydet
                var yeniEtkinlik = new Etkinlik
                {
                    // OyunId tablon olmadığı için 0 veya varsayılan bırakıyoruz
                    EtkinlikAdi = model.EtkinlikAdi, 
                    Tur = model.Tur,
                    SalonId = model.SecilenSalonId,
                    TarihSaat = model.Tarih.Add(model.Saat), // Tarih + Saat birleşimi

                    SatisAktifMi = false
                };

                _dbContext.Etkinlikler.Add(yeniEtkinlik);
                
                // Salonu Aktif'e çek (etkinlik atandığı için)
                var salonToActivate = await _dbContext.Salonlar.FindAsync(model.SecilenSalonId);
                if (salonToActivate != null && salonToActivate.Durum != "Aktif")
                {
                    salonToActivate.Durum = "Aktif";
                    _dbContext.Salonlar.Update(salonToActivate);
                }
                
                // SaveChanges çağırıyoruz ki 'yeniEtkinlik.Id' oluşsun
                await _dbContext.SaveChangesAsync(); 

                // 2. ADIM (GÜNCELLENEN KISIM): Salonu ve Bağlı Planını Çek
                // Artık string isimle aramıyoruz. İlişkili tabloyu (.Include) dahil ediyoruz.
                var salon = await _dbContext.Salonlar
                    .Include(s => s.SeatingPlan) // <--- KRİTİK NOKTA: Plan verisini de getir
                    .FirstOrDefaultAsync(s => s.Id == model.SecilenSalonId);

                // 3. ADIM: JSON'dan Biletleri Üret
                if (salon != null && salon.SeatingPlan != null && !string.IsNullOrEmpty(salon.SeatingPlan.PlanJson))
                {
                    // İlişkili tablodan JSON verisini alıyoruz
                    string planJson = salon.SeatingPlan.PlanJson;

                    using (JsonDocument doc = JsonDocument.Parse(planJson))
                    {
                        var root = doc.RootElement;
                        
                        // React yapısında 'koltuklar' dizisi var mı kontrol et
                        if (root.TryGetProperty("koltuklar", out JsonElement koltuklarElement))
                        {
                            var biletListesi = new List<EtkinlikKoltuk>();

                            foreach (var koltuk in koltuklarElement.EnumerateArray())
                            {
                                // React tarafında küçük harf kullanıldığı için property isimleri küçük
                                string blok = koltuk.GetProperty("blok").GetString();
                                string sira = koltuk.GetProperty("sira").GetString();
                                int numara = koltuk.GetProperty("numara").GetInt32();
                                
                                var yeniBilet = new EtkinlikKoltuk
                                {
                                    EtkinlikId = yeniEtkinlik.Id,
                                    Blok = blok,
                                    Sira = sira,
                                    Numara = numara,
                                    KoltukNo = $"{blok}-{sira}{numara}", // Örn: A-C5
                                    Fiyat = 0, // Etkinlik fiyatı varsayılan olur
                                    DoluMu = false
                                };
                                biletListesi.Add(yeniBilet);
                            }

                            // Tüm biletleri tek seferde veritabanına at (Performanslı)
                            await _dbContext.EtkinlikKoltuklari.AddRangeAsync(biletListesi);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }

                // İşlem başarılı mesajı
                TempData["SuccessMessage"] = "Etkinlik ve biletler başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }

            // Validasyon hatası varsa şehir listesini tekrar doldurup sayfayı geri ver
            model.SehirlerListesi = _dbContext.Salonlar
                .Select(s => s.Sehir).Distinct()
                .Select(s => new SelectListItem(s, s)).ToList();

            // Etkinlik listesini de tekrar doldur
            model.MevcutEtkinlikler = _dbContext.Etkinlikler
                .Include(e => e.Salon)
                .OrderByDescending(e => e.TarihSaat)
                .ToList();

            return View(model);
        }

        // AJAX: Şehre Göre Salonları Getir
        [HttpGet]
        public JsonResult GetSalonlarBySehir(string sehir)
        {
            var salonlar = _dbContext.Salonlar
                .Where(s => s.Sehir == sehir) // Sadece şehre göre filtreledik
                .Select(s => new { 
                    id = s.Id, 
                    ad = s.SalonAdi, 
                    kapasite = s.SalonKapasitesi 
                })
                .ToList();
            
            return Json(salonlar);
        }

        // ETKİNLİK SİLME
        public async Task<IActionResult> Sil(int id)
        {
            var etkinlik = await _dbContext.Etkinlikler.FindAsync(id);
            if (etkinlik != null)
            {
                // Önce biletleri sil
                var biletler = _dbContext.EtkinlikKoltuklari.Where(k => k.EtkinlikId == id);
                _dbContext.EtkinlikKoltuklari.RemoveRange(biletler);

                // Sonra etkinliği sil
                _dbContext.Etkinlikler.Remove(etkinlik);
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik silindi.";
            }
            return RedirectToAction("Index");
        }

        // ETKİNLİK DETAY (KOLTUK DÜZENİ)
        public async Task<IActionResult> Detay(int id)
        {
            var etkinlik = await _dbContext.Etkinlikler
                .Include(e => e.Salon)
                .ThenInclude(s => s.SeatingPlan)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            // O etkinliğe ait biletleri çek
            var biletler = await _dbContext.EtkinlikKoltuklari
                .Where(k => k.EtkinlikId == id)
                .ToListAsync();

            // ViewModel olarak anonymous type veya ViewBag kullanabiliriz şimdilik. 
            // Daha temiz olması için Etkinlik modeline biletleri doldurup view'a gönderelim mi? 
            // Veya ViewBag ile basitçe geçelim.
            ViewBag.Biletler = biletler;
            
            return View(etkinlik);
        }

        // ETKİNLİK DÜZENLEME (GET)
        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var etkinlik = await _dbContext.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }

            // Mevcut verileri ViewModel'e aktar
            var model = new EtkinlikEkleViewModel
            {
                EtkinlikAdi = etkinlik.EtkinlikAdi,
                Tur = etkinlik.Tur,
                Tarih = etkinlik.TarihSaat.Date,
                Saat = etkinlik.TarihSaat.TimeOfDay,
                SecilenSalonId = etkinlik.SalonId,
                // Salon ve Şehir listesini tekrar doldurmak gerekirse buraya ekle
                // Ancak düzenlemede salon değiştirmek veri bütünlüğünü bozabilir (biletler silinmeli vs)
                // O yüzden şimdilik salon değişimine izin vermeyelim veya uyarı verelim.
            };

            // ID'yi View'da kullanmak için ViewBag veya Model'e ID alanı eklenmeli.
            // ViewModel'de Id yok, o yüzden TempData veya ViewBag ile taşıyalım şimdilik.
            ViewBag.EtkinlikId = id;

            return View("Duzenle", model); // Ayrı bir Duzenle.cshtml oluşturacağız
        }

        // ETKİNLİK DÜZENLEME (POST)
        [HttpPost]
        public async Task<IActionResult> Duzenle(int id, EtkinlikEkleViewModel model)
        {
            // Dropdown listesinin validation'ı bozmasını engelle
            ModelState.Remove("SehirlerListesi");
            ModelState.Remove("MevcutEtkinlikler");

            var etkinlik = await _dbContext.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                etkinlik.EtkinlikAdi = model.EtkinlikAdi;
                etkinlik.Tur = model.Tur;
                // Tarih/Saat değişince biletlerin güncellenmesi gerekmez (sadece info)
                etkinlik.TarihSaat = model.Tarih.Add(model.Saat);
                
                // Fiyat değişimi sadece YENİ satılacaklara etki etmeli mi? 
                // Yoksa tüm boş biletleri güncellemeli miyiz?
                // Şimdilik sadece etkinlik ana kaydını güncelleyelim.


                _dbContext.Update(etkinlik);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Düzenleme kaydedildi.";
                return RedirectToAction("Index");
            }
            
            ViewBag.EtkinlikId = id;
            return View("Duzenle", model);
        }

        // SATIŞA AÇ
        public async Task<IActionResult> SatisAc(int id)
        {
            var etkinlik = await _dbContext.Etkinlikler.FindAsync(id);
            if (etkinlik != null)
            {
                etkinlik.SatisAktifMi = true;
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik satışa açıldı.";
            }
            
            // If called from Detay page, redirect back to it via Referer
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && referer.Contains("/Detay/"))
            {
                return RedirectToAction("Detay", new { id });
            }
            
            return RedirectToAction("Index");
        }

        // SATIŞI KAPAT
        public async Task<IActionResult> SatisKapat(int id)
        {
            var etkinlik = await _dbContext.Etkinlikler.FindAsync(id);
            if (etkinlik != null)
            {
                etkinlik.SatisAktifMi = false;
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik satışı kapatıldı.";
            }
            return RedirectToAction("Detay", new { id });
        }
    }
}