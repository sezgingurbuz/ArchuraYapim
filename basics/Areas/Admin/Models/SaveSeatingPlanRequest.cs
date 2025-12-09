namespace basics.Areas.Admin.Models
{
    public class SaveSeatingPlanRequest
    {
        public required string SalonAdi { get; set; }
        //React tarafından gönderilen kapasiteyi karşılar
        public int Kapasite { get; set; }
        public string? PlanAdi { get; set; }
        public required object PlanData { get; set; }
    }
}