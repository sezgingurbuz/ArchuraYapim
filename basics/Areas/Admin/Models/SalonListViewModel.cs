using basics.Areas.Admin.Models; // Kendi namespace'ine göre ayarla

namespace basics.Areas.Admin.Models
{
    public class SalonListViewModel
    {
        // Tabloda listelenecek salonlar
        public List<Salon> Salonlar { get; set; } = new List<Salon>();

        // Dropdown'ı doldurmak için planlar (Kapasite bilgisini buradan alacağız)
        public List<SeatingPlan> Planlar { get; set; } = new List<SeatingPlan>();

        // Yeni eklenecek salonu tutacak nesne
        public Salon YeniSalon { get; set; } = new Salon();

        public List<string> MevcutSehirler { get; set; } = new List<string>();
    }
}