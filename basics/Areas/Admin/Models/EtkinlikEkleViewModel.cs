using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace basics.Areas.Admin.Models
{
    public class EtkinlikEkleViewModel
    {
        // Formdan gelecek veriler
        public int SecilenOyunId { get; set; }
        public string EtkinlikAdi { get; set; }
        public string Tur { get; set; }
        public int SecilenSalonId { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }


        // Dropdown İçin Listeler - Model binding'den hariç tut
        [BindNever]
        public List<SelectListItem> SehirlerListesi { get; set; }
        
        // Mevcut Etkinlikler Listesi
        [BindNever]
        public List<Etkinlik> MevcutEtkinlikler { get; set; } = new List<Etkinlik>();
        // Salon listesi boş gelecek, AJAX ile dolacak
    }
}