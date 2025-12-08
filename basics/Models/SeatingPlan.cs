namespace basics.Models;

public class SeatingPlan
{
    public int Id { get; set; }
    public required string SalonAdi { get; set; }
    public string? PlanAdi { get; set; }
    public required string PlanJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
