using ClearPay.Api.Domain;
using ClearPay.Api.Rules;
using ClearPay.Api.Services;
using Xunit;

namespace ClearPay.Tests;

public class PayCalculationServiceTests
{
    private static PayCalculationService BuildService() =>
        new([new PublicHolidayRule(), new OrdinaryRule(), new OvertimeRule()]);

    [Fact]
    public void A_normal_week_splits_into_ordinary_and_overtime()
    {
        var service = BuildService();
        var entries = new List<TimesheetEntry>
        {
            new(new DateOnly(2026, 10, 12), 8m, false),
            new(new DateOnly(2026, 10, 13), 10m, false), // 8 ordinary + 2 overtime
            new(new DateOnly(2026, 10, 14), 8m, false),
        };

        var breakdown = service.Calculate("Jamie Lee", new PayContext(BaseHourlyRate: 25m), entries);

        Assert.Equal(24m, breakdown.HoursFor("Ordinary"));
        Assert.Equal(2m, breakdown.HoursFor("Overtime"));
        Assert.Equal(0m, breakdown.HoursFor("PublicHoliday"));
        Assert.Equal(26m, breakdown.TotalHours);
        Assert.Equal(24m * 25m + 2m * 25m * 1.5m, breakdown.TotalPay);
    }

    [Fact]
    public void A_public_holiday_day_is_not_also_split_into_ordinary_or_overtime()
    {
        var service = BuildService();
        var entries = new List<TimesheetEntry> { new(new DateOnly(2026, 10, 26), 9m, true) };

        var breakdown = service.Calculate("Jamie Lee", new PayContext(BaseHourlyRate: 25m), entries);

        Assert.Equal(9m, breakdown.HoursFor("PublicHoliday"));
        Assert.Equal(0m, breakdown.HoursFor("Ordinary"));
        Assert.Equal(0m, breakdown.HoursFor("Overtime"));
        Assert.Equal(9m * 25m * 2m, breakdown.TotalPay);
    }

    [Fact]
    public void Adding_a_new_rule_does_not_require_changing_the_service()
    {
        // A throwaway rule that pays a flat $5 allowance on any day with hours, to prove
        // the engine only depends on IPayRule, not on the three built-in rules.
        var service = new PayCalculationService(
        [
            new OrdinaryRule(),
            new OvertimeRule(),
            new FlatAllowanceRule(),
        ]);

        var entries = new List<TimesheetEntry> { new(new DateOnly(2026, 10, 12), 4m, false) };
        var breakdown = service.Calculate("Jamie Lee", new PayContext(BaseHourlyRate: 25m), entries);

        Assert.Equal(1, breakdown.Lines.Count(l => l.RuleName == "FlatAllowance"));
        Assert.Equal(105m, breakdown.TotalPay); // 4 * 25 ordinary + 5 allowance
    }

    private class FlatAllowanceRule : IPayRule
    {
        public string RuleName => "FlatAllowance";

        public PayLine? Apply(TimesheetEntry entry, PayContext context) =>
            entry.HoursWorked > 0 ? new PayLine(RuleName, entry.Date, 0m, 0m, 5m) : null;
    }
}
