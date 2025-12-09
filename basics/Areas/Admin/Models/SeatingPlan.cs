namespace basics.Areas.Admin.Models;
public class SeatingPlan
{
    //Oturma planları için gerekli özellikler
    public int Id { get; set; }
    public required string SalonAdi { get; set; }
    public int Kapasite { get; set; }
    public string? PlanAdi { get; set; }
    public required string PlanJson { get; set; }
    public string Durum { get; set; } = "Pasif";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
