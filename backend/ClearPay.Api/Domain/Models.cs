namespace ClearPay.Api.Domain;

/// <summary>One day's worked hours on a timesheet, as entered by the employee.</summary>
public record TimesheetEntry(DateOnly Date, decimal HoursWorked, bool IsPublicHoliday);

/// <summary>The pay settings a calculation runs under. Kept separate from the rules
/// themselves so the same rules work for any employee or rate.</summary>
public record PayContext(
    decimal BaseHourlyRate,
    decimal OrdinaryThresholdHours = 8m,
    decimal OvertimeMultiplier = 1.5m,
    decimal PublicHolidayMultiplier = 2.0m);

/// <summary>One priced slice of a day, produced by a single pay rule.</summary>
public record PayLine(string RuleName, DateOnly Date, decimal Hours, decimal Rate, decimal Pay);

/// <summary>The full result of running a timesheet through the pay rules.</summary>
public class PayBreakdown
{
    public required string EmployeeName { get; init; }
    public required decimal BaseHourlyRate { get; init; }
    public required List<PayLine> Lines { get; init; }

    public decimal TotalHours => Lines.Sum(l => l.Hours);
    public decimal TotalPay => Lines.Sum(l => l.Pay);

    public decimal HoursFor(string ruleName) => Lines.Where(l => l.RuleName == ruleName).Sum(l => l.Hours);
    public decimal PayFor(string ruleName) => Lines.Where(l => l.RuleName == ruleName).Sum(l => l.Pay);
}
