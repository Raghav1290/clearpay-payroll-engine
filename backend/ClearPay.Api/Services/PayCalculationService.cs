using ClearPay.Api.Domain;
using ClearPay.Api.Rules;

namespace ClearPay.Api.Services;

/// <summary>
/// Runs a timesheet through every registered pay rule. The service knows nothing about
/// what the rules are or how many there are, so new rules register themselves through DI
/// without this class changing.
/// </summary>
public class PayCalculationService(IEnumerable<IPayRule> rules)
{
    public PayBreakdown Calculate(string employeeName, PayContext context, IReadOnlyList<TimesheetEntry> entries)
    {
        var lines = new List<PayLine>();

        foreach (var entry in entries)
        {
            foreach (var rule in rules)
            {
                var line = rule.Apply(entry, context);
                if (line is not null)
                {
                    lines.Add(line);
                }
            }
        }

        return new PayBreakdown
        {
            EmployeeName = employeeName,
            BaseHourlyRate = context.BaseHourlyRate,
            Lines = lines.OrderBy(l => l.Date).ToList(),
        };
    }
}
