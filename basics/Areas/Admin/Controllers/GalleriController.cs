using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using basics.Data;
using basics.Areas.Admin.Models;

namespace basics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin,Editor")]
    public class GalleriController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public GalleriController(ApplicationDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        // Galeri Listesi
        [HttpGet]
        public IActionResult Index()
        {
            var images = _dbContext.GalleryImages
                .OrderByDescending(i => i.UploadedAt)
                .ToList();
            return View(images);
        }

        // Fotoğraf Yükle
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string? title)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return RedirectToAction("Index");
            }

            // Sadece resim dosyalarına izin ver
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Sadece resim dosyaları yüklenebilir (jpg, jpeg, png, gif, webp).";
                return RedirectToAction("Index");
            }

            // Uploads klasörünü oluştur
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "gallery");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Benzersiz dosya adı oluştur
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Dosyayı kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Veritabanına kaydet
            var galleryImage = new GalleryImage
            {
                ImagePath = $"/uploads/gallery/{uniqueFileName}",
                Title = title,
                UploadedAt = DateTime.Now
            };

            _dbContext.GalleryImages.Add(galleryImage);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Fotoğraf başarıyla yüklendi.";
            return RedirectToAction("Index");
        }

        // Fotoğraf Sil
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _dbContext.GalleryImages.FindAsync(id);
            if (image != null)
            {
                // Dosyayı fiziksel olarak sil
                var filePath = Path.Combine(_environment.WebRootPath, image.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Veritabanından sil
                _dbContext.GalleryImages.Remove(image);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Fotoğraf silindi.";
            }
            return RedirectToAction("Index");
        }
    }
}
