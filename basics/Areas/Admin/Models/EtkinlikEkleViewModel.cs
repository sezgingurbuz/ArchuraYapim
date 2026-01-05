using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace basics.Areas.Admin.Models
{
    public class EtkinlikEkleViewModel
    {
        // Formdan gelecek veriler
        public int SecilenOyunId { get; set; }

        [Required(ErrorMessage = "Etkinlik adı zorunludur")]
        [StringLength(200, ErrorMessage = "Etkinlik adı en fazla 200 karakter olabilir")]
        [Display(Name = "Etkinlik Adı")]
        public string EtkinlikAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür seçimi zorunludur")]
        [Display(Name = "Etkinlik Türü")]
        public string Tur { get; set; } = string.Empty;

        [Required(ErrorMessage = "Salon seçimi zorunludur")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir salon seçiniz")]
        [Display(Name = "Salon")]
        public int SecilenSalonId { get; set; }

        [Required(ErrorMessage = "Tarih seçimi zorunludur")]
        [Display(Name = "Etkinlik Tarihi")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Saat seçimi zorunludur")]
        [Display(Name = "Etkinlik Saati")]
        public TimeSpan Saat { get; set; }

        // Satış Tipi: "Koltuk" veya "Kategori"
        [Required(ErrorMessage = "Satış tipi seçimi zorunludur")]
        [Display(Name = "Satış Tipi")]
        public string SatisTipi { get; set; } = "Koltuk";

        // Kategori Satışı İçin - JSON string olarak gelecek
        public string? KategorilerJson { get; set; }

        // Dropdown İçin Listeler - Model binding'den hariç tut
        [BindNever]
        public List<SelectListItem> SehirlerListesi { get; set; } = new();
        
        // Mevcut Etkinlikler Listesi
        [BindNever]
        public List<Etkinlik> MevcutEtkinlikler { get; set; } = new List<Etkinlik>();
        // Salon listesi boş gelecek, AJAX ile dolacak
    }

    // Kategori item helper class
    public class KategoriItemViewModel
    {
        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        public string KategoriAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, 100000, ErrorMessage = "Fiyat 0 ile 100.000 arasında olmalıdır")]
        public decimal Fiyat { get; set; }

        [Range(1, 10000, ErrorMessage = "Kontenjan 1 ile 10.000 arasında olmalıdır")]
        public int? Kontenjan { get; set; } // null = sınırsız
    }
}
