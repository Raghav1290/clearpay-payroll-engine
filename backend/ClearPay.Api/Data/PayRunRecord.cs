namespace ClearPay.Api.Data;

/// <summary>A saved pay run. The priced lines are stored as JSON rather than a child table,
/// since they are only ever read back as a whole breakdown, never queried individually.</summary>
public class PayRunRecord
{
    public int Id { get; set; }
    public required string EmployeeName { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalPay { get; set; }
    public required string LinesJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
