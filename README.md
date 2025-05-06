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
├── appsettings.json             # DB connection config
├── CdrApix.csproj
└── Program.cs
```

---

## 📊 Insight Endpoints
To test endpoints, launch the application and navigate to Swagger UI at `https://localhost:{your-port}/swagger`, where you can try requests like `GET /api/cdr/average-cost` and others interactively.

| Endpoint                          | Route                                         | Description                                        |
|----------------------------------|-----------------------------------------------|----------------------------------------------------|
| **Upload CSV**                   | `POST /api/cdr/upload`                        | Uploads and saves CDRs from a CSV file            |
| **Average Call Cost**            | `GET /api/cdr/average-cost`                  | Returns the average cost of all calls             |
| **Max Call Cost**                | `GET /api/cdr/max-cost`                      | Highest single call cost                          |
| **Longest Duration Call**        | `GET /api/cdr/longest-call`                  | Call with the longest duration                    |
| **Average Calls Per Day**        | `GET /api/cdr/average-calls-per-day`         | Average number of calls per day                   |
| **Total Cost by Currency**       | `GET /api/cdr/total-cost-by-currency`        | Total call costs grouped by currency              |
| **Top N Callers**                | `GET /api/cdr/top-callers?n=5`               | Top N callers with most call records              |
| **Daily Summary**                | `GET /api/cdr/daily-summary`                 | Total calls, duration, and cost grouped by date   |
| **Call Count in Date Range**     | `GET /api/cdr/count?start=...&end=...`       | Number of calls between two specific dates        |
| **Total Duration by Recipient**  | `GET /api/cdr/total-duration?recipient=...`  | Total call duration for a given recipient         |

---

## 🧪 Features

- Accepts CDRs via CSV  
- Entity validation & EF Core integration  
- Insightful analytic endpoints  
- Ignores `.vs/`, `bin/`, `obj/`, and local config using `.gitignore`  
- README and repo structure GitHub-ready

---

## 🔧 Setup (Local)

```bash
git clone https://github.com/asadkayani3240/cdr-apix.git
cd cdr-apix
dotnet restore
dotnet build
```

---

## 💾 Configure the Database

Make sure your connection string is set in `appsettings.json` under `"ConnectionStrings"`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CdrDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

## 🧱 Run EF Core Migrations

If you're setting up the project for the first time, run the following to apply migrations and create the database schema:

```bash
dotnet ef database update
```

> 🛠 If no migrations exist yet, generate the initial one with:

```bash
dotnet ef migrations add InitialCreate
```

---

## 🚀 Run the Application

```bash
dotnet run
```

Visit the Swagger UI at `https://localhost:{your-port}/swagger` to test the API.

> ℹ️ The port number may vary based on your `launchSettings.json` configuration.

---
 
