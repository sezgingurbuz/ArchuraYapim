using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Koltuk Düzeni (Plan)")]
        public string KoltukDuzeni { get; set; } = string.Empty; // SeatingPlan tablosundaki PlanAdi buraya gelecek

        [Display(Name = "Kapasite")]
        public int SalonKapasitesi { get; set; } // Plan seçilince otomatik dolacak
        public string Durum { get; set; } = "Pasif"; // Varsayılan olarak "Pasif"
    }
}