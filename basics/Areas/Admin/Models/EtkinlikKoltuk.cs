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
        public string KoltukNo { get; set; } // Örn: A-1, B-5
        public string Blok { get; set; } // Örn: A, B, Balkon
        public string Sira { get; set; }
        public int Numara { get; set; }

        // Satış Durumu
        public bool DoluMu { get; set; } = false;
        public int? UserId { get; set; } // Alan kişi (Satıldığında dolacak)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Fiyat { get; set; }
        
        // Müşteri Bilgileri
        public string? MusteriAdi { get; set; }
        public string? MusteriSoyadi { get; set; }
        public string? MusteriTelefon { get; set; }
        public string? MusteriEmail { get; set; }
        public string? OdemeYontemi { get; set; } // Nakit, Kart, EFT
        public DateTime? SatisTarihi { get; set; }
        public string? SatisYapanKullanici { get; set; } // Satışı yapan admin kullanıcısı
        public string? SatisPlatformu { get; set; } // Archura, Bubilet, Biletinial
    }
}