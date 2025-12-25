using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace basics.Areas.Admin.Models
{
    public class Salon
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur.")]
        [Display(Name = "Salon Adı")]
        public string SalonAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şehir zorunludur.")]
        public string Sehir { get; set; } = string.Empty;



        [Display(Name = "Koltuk Düzeni")]
        public int? SeatingPlanId { get; set; } // İlişki ID'si

        [ForeignKey("SeatingPlanId")]
        public SeatingPlan SeatingPlan { get; set; } // Navigation Property (Veriye erişmek için)

        [Display(Name = "Kapasite")]
        public int SalonKapasitesi { get; set; } // Plan seçilince otomatik dolacak
        public string Durum { get; set; } = "Pasif"; // Varsayılan olarak "Pasif"
    }
}