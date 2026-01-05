using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using basics.Models;
using basics.Data;

namespace basics.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        // Aktif etkinlikleri tarihe göre artan sırada getir (en yakın etkinlik başta)
        var etkinlikler = _dbContext.Etkinlikler
            .Include(e => e.Salon)
            .Where(e => e.SatisAktifMi && e.TarihSaat >= DateTime.Now)
            .OrderBy(e => e.TarihSaat)
            .ToList();
        
        return View(etkinlikler);
    }

    public IActionResult About()
    {
        ViewData["Title"] = "Hakkımızda";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string? errorId = null)
    {
        ViewBag.ErrorId = errorId;
        return View();
    }
}
