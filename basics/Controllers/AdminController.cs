using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using basics.Models;
using basics.Data;

namespace basics.Controllers;

[Route("[controller]")]
[ApiController]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public AdminController(ILogger<AdminController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("salonOlustur")]
    public IActionResult SalonOlustur()
    {
        return View();
    }

    // Absolute paths keep API routes under /api/admin regardless of class route
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
                    PlanAdi = request.PlanAdi ?? request.SalonAdi,
                    PlanJson = planJson,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _dbContext.SeatingPlans.Add(newPlan);
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { 
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
            return Ok(new { 
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
                .Select(p => new { 
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

public class SaveSeatingPlanRequest
{
    public required string SalonAdi { get; set; }
    public string? PlanAdi { get; set; }
    public required object PlanData { get; set; }
}
