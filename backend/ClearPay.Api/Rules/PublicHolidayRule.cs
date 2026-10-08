using ClearPay.Api.Domain;

namespace ClearPay.Api.Rules;

/// <summary>Hours worked on a day the employee flagged as a public holiday are paid in full
/// at the holiday multiplier. This rule takes the whole day, so Ordinary and Overtime skip it.</summary>
public class PublicHolidayRule : IPayRule
{
    public string RuleName => "PublicHoliday";

    public PayLine? Apply(TimesheetEntry entry, PayContext context)
    {
        if (!entry.IsPublicHoliday || entry.HoursWorked <= 0)
        {
            return null;
        }

        var rate = context.BaseHourlyRate * context.PublicHolidayMultiplier;
        return new PayLine(RuleName, entry.Date, entry.HoursWorked, rate, entry.HoursWorked * rate);
    }
}
