using ClearPay.Api.Data;
using ClearPay.Api.Rules;
using ClearPay.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=clearpay.db"));

// Each pay rule registers itself here. The calculation service and the engine
// that drives it never need to change when a rule is added, changed or removed.
builder.Services.AddScoped<IPayRule, PublicHolidayRule>();
builder.Services.AddScoped<IPayRule, OrdinaryRule>();
builder.Services.AddScoped<IPayRule, OvertimeRule>();
builder.Services.AddScoped<PayCalculationService>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (KeyNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.MapControllers();

app.Run();
