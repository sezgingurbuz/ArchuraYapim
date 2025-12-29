using basics.Areas.Admin.Models;

namespace basics.Models
{
    public class BiletGrubu
    {
        public List<EtkinlikKoltuk> Biletler { get; set; } = new();
        public Etkinlik? Etkinlik { get; set; }
        public DateTime? SatisTarihi { get; set; }
        public decimal ToplamFiyat { get; set; }
        public string Koltuklar { get; set; } = string.Empty;
        
        public int BiletSayisi => Biletler.Count;
    }
}
