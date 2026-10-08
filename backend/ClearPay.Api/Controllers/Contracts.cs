namespace ClearPay.Api.Controllers;

public record TimesheetEntryRequest(DateOnly Date, decimal HoursWorked, bool IsPublicHoliday);

public record CalculatePayRequest(string EmployeeName, decimal BaseHourlyRate, List<TimesheetEntryRequest> Entries);

public record PayLineDto(string RuleName, DateOnly Date, decimal Hours, decimal Rate, decimal Pay);

public record PayBreakdownDto(
    int? Id,
    string EmployeeName,
    decimal BaseHourlyRate,
    decimal TotalHours,
    decimal TotalPay,
    decimal OrdinaryHours,
    decimal OrdinaryPay,
    decimal OvertimeHours,
    decimal OvertimePay,
    decimal PublicHolidayHours,
    decimal PublicHolidayPay,
    List<PayLineDto> Lines);

public record PayRunSummaryDto(int Id, string EmployeeName, decimal TotalHours, decimal TotalPay, DateTime CreatedAtUtc);
