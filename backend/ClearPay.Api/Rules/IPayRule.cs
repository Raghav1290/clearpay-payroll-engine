using ClearPay.Api.Domain;

namespace ClearPay.Api.Rules;

/// <summary>
/// One pay rule decides whether, and how much of, a day's hours it is responsible for pricing.
/// Each rule is independent and stateless, so a new rule (weekend rate, a different allowance,
/// a shift loading) can be added without touching the engine or any existing rule.
/// </summary>
public interface IPayRule
{
    string RuleName { get; }

    /// Returns the line this rule accounts for, or null if it has nothing to price on this entry.
    PayLine? Apply(TimesheetEntry entry, PayContext context);
}
