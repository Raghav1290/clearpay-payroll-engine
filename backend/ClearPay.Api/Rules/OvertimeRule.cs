using ClearPay.Api.Domain;

namespace ClearPay.Api.Rules;

/// <summary>Anything worked past the ordinary-hours threshold on a normal day, paid at the
/// overtime multiplier. Does not apply on a day flagged as a public holiday, where
/// <see cref="PublicHolidayRule"/> prices the whole day instead.</summary>
public class OvertimeRule : IPayRule
{
    public string RuleName => "Overtime";

    public PayLine? Apply(TimesheetEntry entry, PayContext context)
    {
        if (entry.IsPublicHoliday)
        {
            return null;
        }

        var hours = Math.Max(0, entry.HoursWorked - context.OrdinaryThresholdHours);
        if (hours <= 0)
        {
            return null;
        }

        var rate = context.BaseHourlyRate * context.OvertimeMultiplier;
        return new PayLine(RuleName, entry.Date, hours, rate, hours * rate);
    }
}
