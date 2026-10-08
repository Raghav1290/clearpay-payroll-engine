using System.Text.Json;
using ClearPay.Api.Data;
using ClearPay.Api.Domain;
using ClearPay.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Api.Controllers;

[ApiController]
[Route("api/payruns")]
public class PayRunsController(PayCalculationService calculator, AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PayBreakdownDto>> Calculate([FromBody] CalculatePayRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeName))
        {
            return BadRequest(new { error = "Employee name is required." });
        }
        if (request.Entries.Count == 0)
        {
            return BadRequest(new { error = "At least one timesheet entry is required." });
        }
        if (request.Entries.Any(e => e.HoursWorked < 0 || e.HoursWorked > 24))
        {
            return BadRequest(new { error = "Hours worked on a single day must be between 0 and 24." });
        }

        var entries = request.Entries
            .Select(e => new TimesheetEntry(e.Date, e.HoursWorked, e.IsPublicHoliday))
            .ToList();

        var breakdown = calculator.Calculate(request.EmployeeName, new PayContext(request.BaseHourlyRate), entries);
        var dto = ToDto(breakdown, id: null);

        var record = new PayRunRecord
        {
            EmployeeName = breakdown.EmployeeName,
            BaseHourlyRate = breakdown.BaseHourlyRate,
            TotalHours = breakdown.TotalHours,
            TotalPay = breakdown.TotalPay,
            LinesJson = JsonSerializer.Serialize(dto.Lines),
            CreatedAtUtc = DateTime.UtcNow,
        };
        db.PayRuns.Add(record);
        await db.SaveChangesAsync(ct);

        return Ok(dto with { Id = record.Id });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PayRunSummaryDto>>> List(CancellationToken ct)
    {
        var runs = await db.PayRuns
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new PayRunSummaryDto(p.Id, p.EmployeeName, p.TotalHours, p.TotalPay, p.CreatedAtUtc))
            .ToListAsync(ct);
        return Ok(runs);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PayBreakdownDto>> Get(int id, CancellationToken ct)
    {
        var record = await db.PayRuns.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (record is null)
        {
            return NotFound();
        }

        var lines = JsonSerializer.Deserialize<List<PayLineDto>>(record.LinesJson) ?? [];
        return Ok(new PayBreakdownDto(
            record.Id,
            record.EmployeeName,
            record.BaseHourlyRate,
            record.TotalHours,
            record.TotalPay,
            lines.Where(l => l.RuleName == "Ordinary").Sum(l => l.Hours),
            lines.Where(l => l.RuleName == "Ordinary").Sum(l => l.Pay),
            lines.Where(l => l.RuleName == "Overtime").Sum(l => l.Hours),
            lines.Where(l => l.RuleName == "Overtime").Sum(l => l.Pay),
            lines.Where(l => l.RuleName == "PublicHoliday").Sum(l => l.Hours),
            lines.Where(l => l.RuleName == "PublicHoliday").Sum(l => l.Pay),
            lines));
    }

    private static PayBreakdownDto ToDto(PayBreakdown b, int? id) => new(
        id,
        b.EmployeeName,
        b.BaseHourlyRate,
        b.TotalHours,
        b.TotalPay,
        b.HoursFor("Ordinary"),
        b.PayFor("Ordinary"),
        b.HoursFor("Overtime"),
        b.PayFor("Overtime"),
        b.HoursFor("PublicHoliday"),
        b.PayFor("PublicHoliday"),
        b.Lines.Select(l => new PayLineDto(l.RuleName, l.Date, l.Hours, l.Rate, l.Pay)).ToList());
}
