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



        // Bu etkinlik oluşturulduğunda biletler satışa açıldı mı?
        public bool SatisAktifMi { get; set; } = false;
    }

}