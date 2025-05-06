# CDR API (.NET 8)

This is a Call Detail Record (CDR) processing API built in **C# using .NET 8**.  
It supports CSV upload, validation, and storage into a SQL Server database via Entity Framework Core.

---

## 📝 Requirements
- .NET 8 SDK
- SQL Server (local or remote)
- Visual Studio 2022+
- Git
---

## 📁 Project Structure

```text
CdrApix/
├── Controllers/
│   └── CdrController.cs         # Handles file uploads
├── Models/
│   ├── CdrRecord.cs             # Entity model
│   └── CdrRecordMap.cs          # CSV-to-model mapping
├── Data/
│   └── CdrDbContext.cs          # EF DbContext
├── appsettings.json             # DB connection config
├── CdrApix.csproj
└── Program.cs

---
## 📤 CSV Upload Endpoint

- **Route:** `POST /api/Cdr/upload`  
- **Content-Type:** `multipart/form-data`  
- **Field name:** `file`

### Example using `curl`:

```bash
curl -X POST https://localhost:7119/api/Cdr/upload \
  -H "accept: */*" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@techtest_cdr.csv"
```

---

## 🧪 Features

- Accepts CDRs via CSV
- Entity validation & EF Core integration
- Ignores `.vs/`, `bin/`, `obj/`, and local config using `.gitignore`
- README and repo structure GitHub-ready

---

## 🔧 Setup (Local)

git clone https://github.com/asadkayani3240/cdr-apix.git
cd cdr-apix
dotnet restore
dotnet build
dotnet run

Make sure your database connection string is set properly in `appsettings.json`.
---
