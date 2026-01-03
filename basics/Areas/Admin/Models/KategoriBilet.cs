using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace basics.Areas.Admin.Models
{
    /// <summary>
    /// Kategori bazlı satılan biletleri temsil eder.
    /// Kullanıcı online'da kategori seçip ödeme yapar, gişede koltuk ataması yapılır.
    /// </summary>
    public class KategoriBilet
    {
        [Key]
        public int Id { get; set; }

        // Hangi Kategoriden?
        public int EtkinlikKategoriId { get; set; }
        [ForeignKey("EtkinlikKategoriId")]
        public EtkinlikKategori EtkinlikKategori { get; set; }

        // Benzersiz Rezervasyon Kodu (Gişede bu kod sorulacak)
        [Required]
        [Display(Name = "Rezervasyon Kodu")]
        public string RezervasyonKodu { get; set; } // Örn: "RZV-ABC123"

        // Müşteri Bilgileri
        public int? UserId { get; set; } // Üye kullanıcı ise

        [Required]
        [Display(Name = "Müşteri Adı")]
        public string MusteriAdi { get; set; }

        [Required]
        [Display(Name = "Müşteri Soyadı")]
        public string MusteriSoyadi { get; set; }

        [Required]
        [Display(Name = "Telefon")]
        public string MusteriTelefon { get; set; }

        [Display(Name = "E-posta")]
        public string? MusteriEmail { get; set; }

        // Satış Bilgileri
        [Required]
        public DateTime SatisTarihi { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Ödeme Yöntemi")]
        public string OdemeYontemi { get; set; } // Kredi Kartı, Havale vb.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ödenen Fiyat")]
        public decimal OdenenFiyat { get; set; }

        // Koltuk Ataması (Gişede yapılacak)
        public int? AtananKoltukId { get; set; } // EtkinlikKoltuk.Id
        [ForeignKey("AtananKoltukId")]
        public EtkinlikKoltuk? AtananKoltuk { get; set; }

        [Display(Name = "Koltuk Atandı Mı?")]
        public bool KoltukAtandiMi { get; set; } = false;

        [Display(Name = "Koltuk Atama Tarihi")]
        public DateTime? KoltukAtamaTarihi { get; set; }

        // QR Kod - Koltuk atandığında aktif olacak
        public Guid BiletKodu { get; set; } = Guid.NewGuid();

        [Display(Name = "Giriş Yapıldı Mı?")]
        public bool GirisYapildiMi { get; set; } = false;

        // Helper: Tam müşteri adı
        public string MusteriTamAd => $"{MusteriAdi} {MusteriSoyadi}";
    }
}
