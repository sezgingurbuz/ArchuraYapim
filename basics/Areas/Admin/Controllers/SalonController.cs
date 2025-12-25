using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using basics.Data; // Projene göre namespace'i kontrol et
using basics.Areas.Admin.Models; // Projene göre namespace'i kontrol et
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
    public class SalonController : Controller
    {
        private readonly ILogger<SalonController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public SalonController(ILogger<SalonController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        // ==========================================
        // VIEW ACTIONS (Sayfaları Döndüren Metodlar)
        // ==========================================

        // GET: /Admin/Salon/Index
        [HttpGet]
        public IActionResult Index()
        {
            var model = new SalonListViewModel
            {
                // Mevcut salonları getir
                Salonlar = _dbContext.Salonlar
            .Include(x => x.SeatingPlan) 
            .OrderByDescending(x => x.Id)
            .ToList(),

                // Dropdown için planları getir
                Planlar = _dbContext.SeatingPlans.ToList(),
                MevcutSehirler = _dbContext.Salonlar
                    .Select(s => s.Sehir)
                    .Distinct()
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(SalonListViewModel model)
        {
            if (model.YeniSalon.SeatingPlanId > 0 && !string.IsNullOrEmpty(model.YeniSalon.Sehir))
            {
                // 1. Seçilen planı veritabanından bul
                var secilenPlan = await _dbContext.SeatingPlans.FindAsync(model.YeniSalon.SeatingPlanId);

                if (secilenPlan != null)
                {
                    // Kapasiteyi al
                    model.YeniSalon.SalonKapasitesi = secilenPlan.Kapasite;

                    // Planın durumunu 'Aktif' yap (Artık zimmetlendi)
                    secilenPlan.Durum = "Aktif";
                    _dbContext.SeatingPlans.Update(secilenPlan);
                }
                else
                {
                    model.YeniSalon.SalonKapasitesi = 0;
                }

                // Salon varsayılan olarak Pasif başlar (Oyun eklenene kadar)
                model.YeniSalon.Durum = "Pasif";

                _dbContext.Salonlar.Add(model.YeniSalon);

                // Hem Salonu ekle hem Planı güncelle (Transaction gibi çalışır)
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // Hata durumunda listeleri tekrar doldururken de sadece Pasifleri getir
            model.Salonlar = _dbContext.Salonlar.Include(x => x.SeatingPlan).ToList();
            model.Planlar = _dbContext.SeatingPlans.ToList();
    
    return View("Index", model);
        }

        public async Task<IActionResult> Sil(int id)
{
    // İlişkili planı da çekiyoruz (.Include)
    var salon = await _dbContext.Salonlar
        .Include(s => s.SeatingPlan)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (salon != null)
    {
        // İlişkili plan varsa durumunu Pasif yap
        if (salon.SeatingPlan != null)
        {
            salon.SeatingPlan.Durum = "Pasif";
            // SeatingPlan tablosunu güncelle
            _dbContext.SeatingPlans.Update(salon.SeatingPlan);
        }

        _dbContext.Salonlar.Remove(salon);
        await _dbContext.SaveChangesAsync();
    }
    return RedirectToAction("Index");
}

        // Senin özel olarak istediğin sayfa
        // URL: /Admin/Salon/SalonDuzenOlustur
        [HttpGet]
        public IActionResult SalonDuzenOlustur()
        {
            return View();
        }

        // ==========================================
        // API ACTIONS (React/JS'in çağırdığı metodlar)
        // ==========================================
        // Not: Route'lar "/api/..." ile başladığı için Area'dan etkilenmez,
        // frontend kodunu değiştirmene gerek kalmaz.

        [HttpPost("/api/admin/save-seating-plan")]
        public async Task<IActionResult> SaveSeatingPlan([FromBody] SaveSeatingPlanRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SalonAdi))
                    return BadRequest(new { message = "Salon adı gereklidir." });

                if (request.PlanData == null)
                    return BadRequest(new { message = "Plan verisi gereklidir." });

                var existingPlan = _dbContext.SeatingPlans
                    .FirstOrDefault(p => p.SalonAdi == request.SalonAdi);

                var planJson = JsonSerializer.Serialize(request.PlanData);
                var now = DateTime.UtcNow;

                if (existingPlan != null)
                {
                    existingPlan.PlanJson = planJson;
                    existingPlan.UpdatedAt = now;
                    _dbContext.SeatingPlans.Update(existingPlan);
                }
                else
                {
                    var newPlan = new SeatingPlan
                    {
                        SalonAdi = request.SalonAdi,
                        Kapasite = request.Kapasite,
                        PlanAdi = request.PlanAdi ?? request.SalonAdi,
                        PlanJson = planJson,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _dbContext.SeatingPlans.Add(newPlan);
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Plan başarıyla kaydedildi.",
                    salonAdi = request.SalonAdi,
                    savedAt = now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Seating plan kaydetme hatası: {ex.Message}");
                return StatusCode(500, new { message = "Veritabanı kaydetme hatası.", error = ex.Message });
            }
        }

        [HttpGet("/api/admin/get-seating-plan/{salonAdi}")]
        public IActionResult GetSeatingPlan(string salonAdi)
        {
            try
            {
                var plan = _dbContext.SeatingPlans
                    .FirstOrDefault(p => p.SalonAdi == salonAdi);

                if (plan == null)
                    return NotFound(new { message = $"{salonAdi} için salon planı bulunamadı." });

                var planData = JsonSerializer.Deserialize<object>(plan.PlanJson);
                return Ok(new
                {
                    salonAdi = plan.SalonAdi,
                    planAdi = plan.PlanAdi,
                    planData = planData,
                    createdAt = plan.CreatedAt,
                    updatedAt = plan.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Seating plan getirme hatası: {ex.Message}");
                return StatusCode(500, new { message = "Veritabanı getirme hatası.", error = ex.Message });
            }
        }

        [HttpGet("/api/admin/list-seating-plans")]
        public IActionResult ListSeatingPlans()
        {
            try
            {
                var plans = _dbContext.SeatingPlans
                    .Select(p => new
                    {
                        p.Id,
                        p.SalonAdi,
                        p.PlanAdi,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .OrderByDescending(p => p.UpdatedAt)
                    .ToList();

                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Seating plans listesi getirme hatası: {ex.Message}");
                return StatusCode(500, new { message = "Veritabanı listesi getirme hatası.", error = ex.Message });
            }
        }

       

    }
}