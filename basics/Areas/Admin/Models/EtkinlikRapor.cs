using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace basics.Areas.Admin.Models
{
    public class EtkinlikRapor
    {
        [Key]
        public int Id { get; set; }
        
        // Etkinlik Bilgileri
        public string EtkinlikAdi { get; set; }
        public string Tur { get; set; }
        public string SalonAdi { get; set; }
        public string Sehir { get; set; }
        public DateTime TarihSaat { get; set; }
        
        // Satış İstatistikleri
        public int ToplamKapasite { get; set; }
        public int SatilanBilet { get; set; }
        public int BosKoltuk { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ToplamHasilat { get; set; }
        
        // Platform Satışları
        public int BubiletSatisAdedi { get; set; }
        public int BiletinialSatisAdedi { get; set; }
        
        // Ödeme Yöntemi İstatistikleri
        public int NakitSatisAdedi { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal NakitHasilat { get; set; }
        
        public int KartSatisAdedi { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal KartHasilat { get; set; }
        
        public int EFTSatisAdedi { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal EFTHasilat { get; set; }
        
        // Rapor Oluşturulma Tarihi
        public DateTime RaporTarihi { get; set; }
        
        // Raporlayan Kullanıcı
        public string? RaporlayanKullanici { get; set; }
    }
}
