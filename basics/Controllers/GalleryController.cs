using Microsoft.AspNetCore.Mvc;
using basics.Data;

namespace basics.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public GalleryController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var images = _dbContext.GalleryImages
                .OrderByDescending(i => i.UploadedAt)
                .ToList();
            return View(images);
        }
    }
}
