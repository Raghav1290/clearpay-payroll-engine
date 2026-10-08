using ClearPay.Api.Domain;
using ClearPay.Api.Rules;
using Xunit;

namespace ClearPay.Tests;

public class PayRuleTests
{
    private static readonly PayContext Context = new(BaseHourlyRate: 30m);

    [Fact]
    public void Ordinary_rule_pays_full_day_at_base_rate_when_under_threshold()
    {
        var entry = new TimesheetEntry(new DateOnly(2026, 10, 12), HoursWorked: 6m, IsPublicHoliday: false);
        var line = new OrdinaryRule().Apply(entry, Context);

        Assert.NotNull(line);
        Assert.Equal(6m, line.Hours);
        Assert.Equal(30m, line.Rate);
        Assert.Equal(180m, line.Pay);
    }

    [Fact]
    public void Ordinary_rule_caps_at_the_threshold_on_a_long_day()
    {
        var entry = new TimesheetEntry(new DateOnly(2026, 10, 12), HoursWorked: 10m, IsPublicHoliday: false);
        var line = new OrdinaryRule().Apply(entry, Context);

        Assert.NotNull(line);
        Assert.Equal(8m, line.Hours);
    }

    [Fact]
    public void Ordinary_rule_does_not_apply_on_a_public_holiday()
    {
        var entry = new TimesheetEntry(new DateOnly(2026, 10, 12), HoursWorked: 6m, IsPublicHoliday: true);
        Assert.Null(new OrdinaryRule().Apply(entry, Context));
    }

    [Fact]
    public void Overtime_rule_covers_only_hours_past_the_threshold_at_time_and_a_half()
    {
        var entry = new TimesheetEntry(new DateOnly(2026, 10, 12), HoursWorked: 10m, IsPublicHoliday: false);
        var line = new OvertimeRule().Apply(entry, Context);

        Assert.NotNull(line);
        Assert.Equal(2m, line.Hours);
        Assert.Equal(45m, line.Rate); // 30 * 1.5
        Assert.Equal(90m, line.Pay);
    }

    [Fact]
    public void Overtime_rule_does_not_apply_at_exactly_the_threshold()
    {
        var entry = new TimesheetEntry(new DateOnly(2026, 10, 12), HoursWorked: 8m, IsPublicHoliday: false);
        Assert.Null(new OvertimeRule().Apply(entry, Context));
    }

    [Fact]
    public void Public_holiday_rule_pays_every_hour_at_double_time()
    {
        var entry = new TimesheetEntry(new DateOnly(2026, 10, 26), HoursWorked: 5m, IsPublicHoliday: true);
        var line = new PublicHolidayRule().Apply(entry, Context);

        Assert.NotNull(line);
        Assert.Equal(5m, line.Hours);
        Assert.Equal(60m, line.Rate); // 30 * 2.0
        Assert.Equal(300m, line.Pay);
    }

    [Fact]
    public void Zero_hour_entries_produce_no_lines_from_any_rule()
    {
        var entry = new TimesheetEntry(new DateOnly(2026, 10, 12), HoursWorked: 0m, IsPublicHoliday: false);

        Assert.Null(new OrdinaryRule().Apply(entry, Context));
        Assert.Null(new OvertimeRule().Apply(entry, Context));
        Assert.Null(new PublicHolidayRule().Apply(entry, Context));
    }
}
