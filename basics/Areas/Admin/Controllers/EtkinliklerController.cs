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
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin,Editor")]
    public class EtkinliklerController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public EtkinliklerController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // SAYFAYI GETİR (GET)
        [HttpGet]
        public IActionResult Index(int page = 1, string search = "")
        {
            const int pageSize = 10; // Sayfa başına 10 öğe
            
            var query = _dbContext.Etkinlikler
                .Include(e => e.Salon)
                .AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower();
                query = query.Where(e => e.EtkinlikAdi.ToLower().Contains(s) || e.Salon.SalonAdi.ToLower().Contains(s));
            }

            // Toplam kayıt sayısı (Sayfalama için)
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Sayfalama uygula
            var etkinlikler = query
                .OrderByDescending(e => e.TarihSaat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // View'a gerekli verileri gönder
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;

            var model = new EtkinlikEkleViewModel
            {
                // Şehirleri Salonlar tablosundan çekiyoruz (Distinct: Tekrarsız)
                SehirlerListesi = _dbContext.Salonlar
                    .Select(s => s.Sehir)
                    .Distinct()
                    .Select(s => new SelectListItem(s, s))
                    .ToList(),
                
                Tarih = DateTime.Today,
                
                // Mevcut etkinlikleri getir (Paginated liste)
                MevcutEtkinlikler = etkinlikler
            };

            return View(model);
        }

        // ETKİNLİĞİ KAYDET VE BİLETLERİ OLUŞTUR (POST)
        [HttpPost]
        public async Task<IActionResult> Index(EtkinlikEkleViewModel model)
        {
            // Dropdown listesinin validation'ı bozmasını engelle
            ModelState.Remove("SehirlerListesi");
            ModelState.Remove("KategorilerJson");

            if (ModelState.IsValid)
            {
                // 1. ADIM: Etkinliği Oluştur ve Kaydet
                var yeniEtkinlik = new Etkinlik
                {
                    EtkinlikAdi = model.EtkinlikAdi, 
                    Tur = model.Tur,
                    SalonId = model.SecilenSalonId,
                    TarihSaat = model.Tarih.Add(model.Saat),
                    SatisTipi = model.SatisTipi ?? "Koltuk", // Satış tipi
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

                // 2. ADIM: Satış tipine göre işlem yap
                if (model.SatisTipi == "Kategori")
                {
                    // KATEGORİ SATIŞI: Kategorileri oluştur
                    if (!string.IsNullOrEmpty(model.KategorilerJson))
                    {
                        var kategoriler = JsonSerializer.Deserialize<List<KategoriItemViewModel>>(
                            model.KategorilerJson, 
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (kategoriler != null && kategoriler.Any())
                        {
                            foreach (var kat in kategoriler)
                            {
                                var yeniKategori = new EtkinlikKategori
                                {
                                    EtkinlikId = yeniEtkinlik.Id,
                                    KategoriAdi = kat.KategoriAdi,
                                    Fiyat = kat.Fiyat,
                                    Kontenjan = kat.Kontenjan
                                };
                                _dbContext.EtkinlikKategorileri.Add(yeniKategori);
                            }
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                    // KATEGORİ SATIŞI İÇİN DE KOLTUKLARI OLUŞTUR (Gişede koltuk ataması için gerekli)
                    var salonKategori = await _dbContext.Salonlar
                        .Include(s => s.SeatingPlan)
                        .FirstOrDefaultAsync(s => s.Id == model.SecilenSalonId);

                    if (salonKategori != null && salonKategori.SeatingPlan != null && !string.IsNullOrEmpty(salonKategori.SeatingPlan.PlanJson))
                    {
                        string planJson = salonKategori.SeatingPlan.PlanJson;

                        using (JsonDocument doc = JsonDocument.Parse(planJson))
                        {
                            var root = doc.RootElement;
                            
                            if (root.TryGetProperty("koltuklar", out JsonElement koltuklarElement))
                            {
                                var biletListesi = new List<EtkinlikKoltuk>();

                                foreach (var koltuk in koltuklarElement.EnumerateArray())
                                {
                                    string blok = koltuk.GetProperty("blok").GetString();
                                    string sira = koltuk.GetProperty("sira").GetString();
                                    int numara = koltuk.GetProperty("numara").GetInt32();
                                    
                                    var yeniBilet = new EtkinlikKoltuk
                                    {
                                        EtkinlikId = yeniEtkinlik.Id,
                                        Blok = blok,
                                        Sira = sira,
                                        Numara = numara,
                                        KoltukNo = $"{blok}-{sira}{numara}",
                                        Fiyat = 0,
                                        DoluMu = false
                                    };
                                    biletListesi.Add(yeniBilet);
                                }

                                await _dbContext.EtkinlikKoltuklari.AddRangeAsync(biletListesi);
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Etkinlik, kategoriler ve koltuklar başarıyla oluşturuldu.";
                }
                else
                {
                    // KOLTUK SATIŞI: Mevcut davranış - Salon planından koltuk oluştur
                    var salon = await _dbContext.Salonlar
                        .Include(s => s.SeatingPlan)
                        .FirstOrDefaultAsync(s => s.Id == model.SecilenSalonId);

                    if (salon != null && salon.SeatingPlan != null && !string.IsNullOrEmpty(salon.SeatingPlan.PlanJson))
                    {
                        string planJson = salon.SeatingPlan.PlanJson;

                        using (JsonDocument doc = JsonDocument.Parse(planJson))
                        {
                            var root = doc.RootElement;
                            
                            if (root.TryGetProperty("koltuklar", out JsonElement koltuklarElement))
                            {
                                var biletListesi = new List<EtkinlikKoltuk>();

                                foreach (var koltuk in koltuklarElement.EnumerateArray())
                                {
                                    string blok = koltuk.GetProperty("blok").GetString();
                                    string sira = koltuk.GetProperty("sira").GetString();
                                    int numara = koltuk.GetProperty("numara").GetInt32();
                                    
                                    var yeniBilet = new EtkinlikKoltuk
                                    {
                                        EtkinlikId = yeniEtkinlik.Id,
                                        Blok = blok,
                                        Sira = sira,
                                        Numara = numara,
                                        KoltukNo = $"{blok}-{sira}{numara}",
                                        Fiyat = 0,
                                        DoluMu = false
                                    };
                                    biletListesi.Add(yeniBilet);
                                }

                                await _dbContext.EtkinlikKoltuklari.AddRangeAsync(biletListesi);
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Etkinlik ve biletler başarıyla oluşturuldu.";
                }
                
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
                .Include(e => e.Kategoriler)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            // O etkinliğe ait biletleri çek
            var biletler = await _dbContext.EtkinlikKoltuklari
                .Where(k => k.EtkinlikId == id)
                .ToListAsync();

            ViewBag.Biletler = biletler;

            // Kategori satışı ise, atanmamış kategori bilet sayısını hesapla
            if (etkinlik.SatisTipi == "Kategori")
            {
                var kategoriIds = etkinlik.Kategoriler?.Select(k => k.Id).ToList() ?? new List<int>();
                var toplam = await _dbContext.KategoriBiletler
                    .Where(kb => kategoriIds.Contains(kb.EtkinlikKategoriId))
                    .CountAsync();
                var atanmamis = await _dbContext.KategoriBiletler
                    .Where(kb => kategoriIds.Contains(kb.EtkinlikKategoriId) && !kb.KoltukAtandiMi)
                    .CountAsync();
                
                ViewBag.ToplamKategoriBilet = toplam;
                ViewBag.AtanmamisKategoriBilet = atanmamis;
            }
            
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