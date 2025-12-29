using System.ComponentModel.DataAnnotations;

namespace basics.Areas.Admin.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public string? Title { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
