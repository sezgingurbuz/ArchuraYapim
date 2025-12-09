using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using basics.Data; // Projene göre namespace'i kontrol et
using basics.Areas.Admin.Models; // Projene göre namespace'i kontrol et
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        // URL: /Admin veya /Admin/Home/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}