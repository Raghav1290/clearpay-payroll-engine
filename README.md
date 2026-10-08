# ClearPay

A small payroll calculation engine. You enter an employee's timesheet, hour by hour, and it works out ordinary pay, overtime, and public holiday pay, then saves the result.

Built with ASP.NET Core 8, Entity Framework Core, React, TypeScript and xUnit.

## Why this exists

Payroll calculation looks simple from the outside and is not, once you get into overtime thresholds, holiday rates, and the fact that pay rules change over time and between employers. I built ClearPay to work through that kind of problem properly: each pay rule is its own class behind a shared interface, so adding a new rule, say a weekend rate or a different overtime threshold, never means touching the engine or any other rule. That's the open/closed principle in practice, not just a line on a CV.

## How it works

- **Rules (`Rules/`)**: `IPayRule` has one method, `Apply`. Three rules implement it: `OrdinaryRule` (hours up to the threshold at the base rate), `OvertimeRule` (hours past the threshold at 1.5x), and `PublicHolidayRule` (a flagged day paid in full at 2x). Each rule only knows about itself.
- **Engine (`Services/PayCalculationService.cs`)**: takes every registered rule and runs each one against every timesheet entry. It has no idea how many rules exist or what they do.
- **API (`Controllers/PayRunsController.cs`)**: calculates a breakdown, saves it, and can return the history.
- **Frontend**: a form for entering timesheet rows, a breakdown panel, and a list of past pay runs.

## Running it locally

Requirements: .NET 8 SDK and Node.js 20+.

```bash
cd backend/ClearPay.Api
dotnet run            # API on http://localhost:5090

cd frontend
npm install
npm run dev            # open the URL Vite prints
```

Run the tests:

```bash
cd backend
dotnet test
```

## Current limits

- The holiday and overtime rates (2x and 1.5x) and the 8 hour threshold are simplified defaults, not a copy of the Holidays Act or any specific employment agreement. A real system would need those configured per employer, and reviewed by someone who owns payroll compliance, not assumed by a developer.
- Whether a day is a public holiday is entered by hand on each row rather than looked up from a calendar, so the demo doesn't depend on a hardcoded, possibly wrong, list of dates.
- Pay run lines are stored as JSON on the record rather than as their own table, since they are only ever read back as a whole breakdown.
- No authentication. This is a demo, not a product.
