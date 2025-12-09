using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using basics.Data; // Projene göre namespace'i kontrol et
using basics.Areas.Admin.Models; // Projene göre namespace'i kontrol et
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
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
                Salonlar = _dbContext.Salonlar.OrderByDescending(x => x.Id).ToList(),

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
                // Salon adı ve Şehir doluysa işleme başla
                if (!string.IsNullOrEmpty(model.YeniSalon.SalonAdi) && !string.IsNullOrEmpty(model.YeniSalon.Sehir))
                {
                    // 1. Kapasiteyi Plan'dan bul ve ata
                    var secilenPlan = await _dbContext.SeatingPlans
                        .FirstOrDefaultAsync(p => p.PlanAdi == model.YeniSalon.KoltukDuzeni);

                    model.YeniSalon.SalonKapasitesi = secilenPlan != null ? secilenPlan.Kapasite : 0;

                    // 2. YENİ: Durumu açıkça "Pasif" olarak ayarla
                    model.YeniSalon.Durum = "Pasif";

                    // 3. Kaydet
                    _dbContext.Salonlar.Add(model.YeniSalon);
                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("Index");
                }

                // Hata varsa sayfayı tekrar doldur
                model.Salonlar = _dbContext.Salonlar.ToList();
                model.Planlar = _dbContext.SeatingPlans.ToList();
                return View("Index", model);
            
        
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