// Satışın yapılacağı model
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace basics.Areas.Admin.Models
{
    public class EtkinlikKoltuk
    {
        public int Id { get; set; }

        // Hangi Etkinliğe ait?
        public int EtkinlikId { get; set; }
        [ForeignKey("EtkinlikId")]
        public Etkinlik Etkinlik { get; set; }

        // Koltuk Bilgileri (JSON'dan gelecek)
        [Required]
        [StringLength(20)]
        public string KoltukNo { get; set; } // Örn: A-1, B-5

        [StringLength(50)]
        public string Blok { get; set; } // Örn: A, B, Balkon

        [StringLength(10)]
        public string Sira { get; set; }

        public int Numara { get; set; }

        // Satış Durumu
        public bool DoluMu { get; set; } = false;
        public int? UserId { get; set; } // Alan kişi (Satıldığında dolacak)

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000, ErrorMessage = "Fiyat 0 ile 100.000 arasında olmalıdır")]
        public decimal Fiyat { get; set; }
        
        // Müşteri Bilgileri
        [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
        [Display(Name = "Müşteri Adı")]
        public string? MusteriAdi { get; set; }

        [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir")]
        [Display(Name = "Müşteri Soyadı")]
        public string? MusteriSoyadi { get; set; }

        [RegularExpression(@"^(\+90|0)?5[0-9]{9}$", ErrorMessage = "Geçerli bir cep telefonu numarası giriniz")]
        [StringLength(15)]
        [Display(Name = "Telefon")]
        public string? MusteriTelefon { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(255)]
        [Display(Name = "E-posta")]
        public string? MusteriEmail { get; set; }

        [StringLength(50)]
        [Display(Name = "Ödeme Yöntemi")]
        public string? OdemeYontemi { get; set; } // Nakit, Kart, EFT

        public DateTime? SatisTarihi { get; set; }

        [StringLength(100)]
        public string? SatisYapanKullanici { get; set; } // Satışı yapan admin kullanıcısı

        [StringLength(50)]
        public string? SatisPlatformu { get; set; } // Archura, Bubilet, Biletinial

        // QR Kod Doğrulama
        public Guid BiletKodu { get; set; } = Guid.NewGuid(); // Benzersiz bilet tanımlayıcısı
        public bool GirisYapildiMi { get; set; } = false; // Etkinliğe giriş yapıldı mı?
    }
}