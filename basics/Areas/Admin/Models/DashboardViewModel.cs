using System;
using System.Collections.Generic;

namespace basics.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // İstatistikler
        public int ToplamEtkinlik { get; set; }
        public int AktifEtkinlik { get; set; }
        public int TamamlananEtkinlik { get; set; }
        public int ToplamSalon { get; set; }
        public int ToplamSatilanBilet { get; set; }
        public decimal ToplamHasilat { get; set; }
        
        // Yaklaşan Etkinlikler
        public List<YakinEtkinlik> YakinEtkinlikler { get; set; } = new List<YakinEtkinlik>();
        
        // Son Satışlar
        public List<SonSatis> SonSatislar { get; set; } = new List<SonSatis>();
    }
    
    public class YakinEtkinlik
    {
        public int Id { get; set; }
        public string EtkinlikAdi { get; set; }
        public string SalonAdi { get; set; }
        public DateTime TarihSaat { get; set; }
        public int SatilanBilet { get; set; }
        public int ToplamKapasite { get; set; }
        public int DolulukOrani => ToplamKapasite > 0 ? (SatilanBilet * 100 / ToplamKapasite) : 0;
    }
    
    public class SonSatis
    {
        public string EtkinlikAdi { get; set; }
        public string KoltukNo { get; set; }
        public string MusteriAdi { get; set; }
        public decimal Fiyat { get; set; }
        public DateTime SatisTarihi { get; set; }
        public string SatisPlatformu { get; set; }
    }
}
