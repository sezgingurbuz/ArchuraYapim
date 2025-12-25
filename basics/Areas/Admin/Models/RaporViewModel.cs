namespace basics.Areas.Admin.Models
{
    public class RaporViewModel
    {
        public int EtkinlikId { get; set; }
        public string EtkinlikAdi { get; set; }
        public string SalonAdi { get; set; }
        public string Sehir { get; set; }
        public DateTime Tarih { get; set; }
        
        // İstatistikler
        public int ToplamKapasite { get; set; }
        public int SatilanBilet { get; set; }
        public int BosKoltuk { get; set; }
        public decimal ToplamHasilat { get; set; } // Kazanılan Para
        
        // Platform bazlı satışlar
        public int BubiletSatisAdedi { get; set; }
        public int BiletinialSatisAdedi { get; set; }
        
        // Ödeme yöntemi bazlı istatistikler
        public int NakitSatisAdedi { get; set; }
        public decimal NakitHasilat { get; set; }
        public int KartSatisAdedi { get; set; }
        public decimal KartHasilat { get; set; }
        public int EFTSatisAdedi { get; set; }
        public decimal EFTHasilat { get; set; }
        
        // Yüzdelik (Progress Bar için)
        public int DolulukOrani 
        {
            get 
            {
                if (ToplamKapasite == 0) return 0;
                return (int)((double)SatilanBilet / ToplamKapasite * 100);
            }
        }
    }
}