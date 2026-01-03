using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace basics.Areas.Admin.Models
{
    /// <summary>
    /// Etkinliğe ait bilet kategorilerini temsil eder.
    /// Kategori satışı seçildiğinde kullanılır.
    /// </summary>
    public class EtkinlikKategori
    {
        [Key]
        public int Id { get; set; }

        // Hangi Etkinliğe ait?
        public int EtkinlikId { get; set; }
        [ForeignKey("EtkinlikId")]
        public Etkinlik Etkinlik { get; set; }

        // Kategori Bilgileri
        [Required]
        [Display(Name = "Kategori Adı")]
        public string KategoriAdi { get; set; } // VIP, Balkon, Parter vb.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fiyat")]
        public decimal Fiyat { get; set; }

        // Opsiyonel Kontenjan Limiti (null ise sınırsız)
        [Display(Name = "Kontenjan")]
        public int? Kontenjan { get; set; }

        // Navigation Property - Bu kategoriden satılan biletler
        public ICollection<KategoriBilet>? SatilanBiletler { get; set; }
    }
}
