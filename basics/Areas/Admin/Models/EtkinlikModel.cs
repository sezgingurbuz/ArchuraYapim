using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace basics.Areas.Admin.Models
{
    public class Etkinlik
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Etkinlik Adı")]
        public string EtkinlikAdi { get; set; } // Genelde Oyun adıyla aynı olur ama özelleştirilebilir

        // Hangi Salon?
        public int SalonId { get; set; }
        [ForeignKey("SalonId")]
        public Salon Salon { get; set; }

        [Required]
        public DateTime TarihSaat { get; set; } // Gün ve Saat birleşik tutulur

        [Required]
        public string Tur { get; set; } // Tiyatro, Müzikal vb.

        // Satış Tipi: "Koltuk" veya "Kategori"
        [Required]
        public string SatisTipi { get; set; } = "Koltuk";

        // Kategoriler (Navigation Property) - Kategori satışı için
        public ICollection<EtkinlikKategori>? Kategoriler { get; set; }

        // Bu etkinlik oluşturulduğunda biletler satışa açıldı mı?
        public bool SatisAktifMi { get; set; } = false;
    }

}