using ClearPay.Api.Domain;

namespace ClearPay.Api.Rules;

/// <summary>The first block of a normal working day, up to the ordinary-hours threshold,
/// paid at the employee's base rate. Does not apply on a day flagged as a public holiday.</summary>
public class OrdinaryRule : IPayRule
{
    public string RuleName => "Ordinary";

    public PayLine? Apply(TimesheetEntry entry, PayContext context)
    {
        if (entry.IsPublicHoliday)
        {
            return null;
        }

        var hours = Math.Min(entry.HoursWorked, context.OrdinaryThresholdHours);
        if (hours <= 0)
        {
            return null;
        }

        return new PayLine(RuleName, entry.Date, hours, context.BaseHourlyRate, hours * context.BaseHourlyRate);
    }
}
