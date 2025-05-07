# CDR API (.NET 8)

This is a Call Detail Record (CDR) processing API built in **C# using .NET 8**.  
It supports CSV upload, validation, and storage into a SQL Server database via Entity Framework Core.

---
## 📝 Requirements

- .NET 8 SDK  
- SQL Server (local or remote)  
- Visual Studio 2022+  
- Git  
- EF Core CLI (`dotnet tool install --global dotnet-ef`)

---
## 📁 Project Structure

```text
CdrApix/
├── Controllers/
│   └── CdrController.cs         # Handles file uploads and insight endpoints
├── Models/
│   ├── CdrRecord.cs             # Entity model
│   └── CdrRecordMap.cs          # CSV-to-model mapping
├── Data/
│   └── CdrDbContext.cs          # EF DbContext
├── DTOs/
│   ├── CallSummaryDto.cs
│   ├── CostByCurrencyDto.cs
│   └── TopCallerDto.cs
├── Services/
│   └── CdrInsightsService.cs    # Business logic layer
├── scripts/
│   └── CreateCdrDb.sql          # SQL fallback for DB setup
├── appsettings.json             # DB connection config
├── CdrApix.csproj
└── Program.cs
```

---
## Insight Endpoints

To test endpoints, launch the application and navigate to Swagger UI at `https://localhost:{your-port}/swagger`, where you can try requests like `GET /api/cdr/average-cost` and others interactively.

| Endpoint                          | Route                                         | Description                                        |
|----------------------------------|-----------------------------------------------|----------------------------------------------------|
| **Upload CSV**                   | `POST /api/cdr/upload`                        | Uploads and saves CDRs from a CSV file             |
| **Average Call Cost**            | `GET /api/cdr/average-cost`                   | Returns the average cost of all calls              |
| **Max Call Cost**                | `GET /api/cdr/max-cost`                       | Highest single call cost                           |
| **Longest Duration Call**        | `GET /api/cdr/longest-call`                   | Call with the longest duration                     |
| **Average Calls Per Day**        | `GET /api/cdr/average-calls-per-day`          | Average number of calls per day                    |
| **Total Cost by Currency**       | `GET /api/cdr/total-cost-by-currency`         | Total call costs grouped by currency               |
| **Top N Callers**                | `GET /api/cdr/top-callers?n=5`                | Top N callers with most call records               |
| **Daily Summary**                | `GET /api/cdr/daily-summary`                  | Total calls, duration, and cost grouped by date    |
| **Call Count in Date Range**     | `GET /api/cdr/count?start=...&end=...`        | Number of calls between two specific dates         |
| **Total Duration by Recipient**  | `GET /api/cdr/total-duration?recipient=...`   | Total call duration for a given recipient          |

---
## Features

- Accepts CDRs via CSV  
- Entity validation & EF Core integration  
- Insightful analytic endpoints  
- Clean architecture: DTOs, service layer, thin controllers  
- Ignores `.vs/`, `bin/`, `obj/`, and local config via `.gitignore`  
- Self-documenting Swagger UI

---
## Setup (Local)

```bash
git clone https://github.com/asadkayani3240/cdr-apix.git
cd cdr-apix
dotnet restore
dotnet build
```

---
## Configure the Database

Ensure your connection string is set in `appsettings.json` under `"ConnectionStrings"`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=XX;Database=CdrDb;User Id=XX;Password=XX;"
}
``` 

---
## Run EF Core Migrations

If setting up for the first time, apply migrations and create the database schema:

```bash
dotnet ef database update
```

> 🛠 If no migrations exist yet, generate the initial one:

```bash
dotnet ef migrations add InitialCreate
```

---
## SQL Fallback Script

If you encounter issues running EF Core migrations, you can manually create the database and table by executing the provided SQL script:

```bash
# From the project root
dotnet tool install --global sqlcmd
sqlcmd -S <server> -d master -i scripts/CreateCdrDb.sql
```

This script will:
1. Create the `CdrDb` database if it doesn't exist.
2. Create the `dbo.CdrRecords` table with the correct schema.

---
## Run the Application

```bash
dotnet run
```

Visit the Swagger UI at `https://localhost:{your-port}/swagger` to test the API.

> The port number may vary based on your `launchSettings.json` configuration.

---
## 🛠 Technology Choices

- **.NET 8**: Latest C# features and LTS support  
- **Entity Framework Core**: Code-first models and migrations  
- **CsvHelper**: Robust CSV parsing and mapping  
- **xUnit & EF InMemory**: Fast, isolated unit tests  
- **Swagger (Swashbuckle)**: Auto-generated API documentation

---
## 🤔 Assumptions

- Single-tenant, no authentication/authorization  
- Dataset fits in memory; CSV uploads are moderate size  
- No concurrent upload conflict handling; overwrites allowed

---
## 🚀 Future Enhancements

- **Authentication/Authorization**: JWT or API key support  
- **Pagination & filtering**: Add query parameters for insight endpoints  
- **Integration Testing**: Add end-to-end tests using `WebApplicationFactory<Program>` to validate HTTP routes, middleware, and JSON contracts.
