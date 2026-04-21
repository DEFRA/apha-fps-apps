# Batch Jobs - VM Test Quick Reference

## 🚀 Test 1: Run WITHOUT Database (NoDb Mode)

**No PostgreSQL required!**

```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- HealthCheck
```

**Expected Result:**
- ✅ Exit Code: 0
- ✅ Duration: ~4 seconds
- ✅ Status: Completed
- ✅ Database: In-Memory

---

## 🗄️ Test 2: Run WITH Database (WithDb Mode)

**PostgreSQL required!**

### Prerequisites:
1. PostgreSQL service running: `Get-Service -Name "*postgres*"`
2. Database `batchjobs` exists
3. Connection string in `appsettings.Development.json`

### Run:
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run -- HealthCheck
```

**Expected Result:**
- ✅ Exit Code: 0
- ✅ Duration: ~13 seconds
- ✅ Status: Completed
- ✅ Database: PostgreSQL (localhost:5432)
- ✅ Lock acquired and released
- ✅ Records persisted to database

---

## 📋 Quick Comparison

| Aspect | Test 1 (NoDb) | Test 2 (WithDb) |
|--------|---------------|-----------------|
| Environment | `Demo` | `Development` |
| PostgreSQL | Not Required | Required |
| Duration | ~4 seconds | ~13 seconds |
| Persistence | In-Memory | PostgreSQL |
| Use Case | Testing/Demos | Production-like |

---

## ⚡ Interactive Mode

```powershell
.\run-demo.ps1
# Select: 1 for NoDb, 3 for WithDb
```

---

## 🔍 Verify Success

Look for in console output:
```
Outcome=Succeeded
ExitCode=0
Status=Completed
RecordsProcessed=50
GracefulShutdownCompleted=True
```

---

## 🛠️ Troubleshooting

**Test 1 fails?**
- Check .NET 10 SDK installed: `dotnet --version`
- Verify you're in correct directory

**Test 2 fails?**
- Check PostgreSQL: `Get-Service -Name "*postgres*"`
- Verify database exists: Connect to postgres and list databases
- Check connection string in `appsettings.Development.json`

---

**Both tests working = ✅ Ready for production deployment!**
