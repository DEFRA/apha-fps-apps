# Batch Jobs Testing Guide - Where to Run What

## 🏗️ Environment Overview

| Environment | Technology | Docker Available? | Guide to Use |
|-------------|-----------|-------------------|--------------|
| **VM (Windows Server)** | .NET 10 SDK | ❌ NO | `Apha.BatchJobs.Worker\VM-TESTING-GUIDE.md` |
| **GitHub Codespaces** | Docker + .NET 10 | ✅ YES | `CONTAINER-TESTING-GUIDE.md` |

---

## 🖥️ VM Environment (Your Current Environment)

**Location:** `D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation`

**What's Available:**
- ✅ .NET 10 SDK
- ✅ PostgreSQL 16
- ✅ Visual Studio 2026
- ❌ Docker (NOT installed)

**How to Test:**

### Test 1: VM Without Database (NoDb)
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- HealthCheck
```

### Test 2: VM With Database (WithDb)
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run -- HealthCheck
```

**Prerequisites for Test 2:**
- Create `appsettings.local.json` with your database password
- PostgreSQL service running

📖 **Full Guide:** `Apha.BatchJobs.Worker\VM-TESTING-GUIDE.md`

---

## ☁️ GitHub Codespaces Environment

**Location:** Cloud-based development environment

**What's Available:**
- ✅ .NET 10 SDK
- ✅ Docker
- ✅ docker-compose
- ✅ Pre-configured PostgreSQL container

**How to Test:**

### Test 1: Container Without Database (NoDb)
```bash
cd src/Apha.BatchJobs
docker-compose --profile nodb up batch-jobs-nodb
```

### Test 2: Container With Database (WithDb)
```bash
cd src/Apha.BatchJobs
docker-compose --profile withdb up
```

**No Prerequisites Needed:**
- Database auto-initializes
- All configuration via environment variables
- No local files needed

📖 **Full Guide:** `CONTAINER-TESTING-GUIDE.md`

---

## 🎯 Quick Decision Matrix

**Where am I?** → **What should I use?**

### If you see this in your terminal:
```
PS D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation>
```
→ **You're in VM** → Use `dotnet run` (see `VM-TESTING-GUIDE.md`)

### If you see this in your terminal:
```
@username ➜ /workspaces/apha-fps-apps $
```
→ **You're in Codespaces** → Use `docker-compose` (see `CONTAINER-TESTING-GUIDE.md`)

---

## ❌ Common Mistakes

### ❌ WRONG: Running Docker commands in VM
```powershell
# This will FAIL in VM
docker-compose up
```
**Error:** `docker-compose : The term 'docker-compose' is not recognized`

**Why?** Docker is not installed in the VM environment.

### ✅ RIGHT: Running .NET directly in VM
```powershell
# This WORKS in VM
dotnet run -- HealthCheck
```

---

## 📊 All Test Scenarios Summary

| # | Environment | Mode | Technology | Database | Duration | Guide |
|---|-------------|------|-----------|----------|----------|-------|
| 1 | VM | NoDb | .NET SDK | In-Memory | ~4s | `VM-TESTING-GUIDE.md` |
| 2 | VM | WithDb | .NET SDK | PostgreSQL | ~17s | `VM-TESTING-GUIDE.md` |
| 3 | Codespaces | NoDb | Docker | In-Memory | ~4s | `CONTAINER-TESTING-GUIDE.md` |
| 4 | Codespaces | WithDb | Docker | PostgreSQL | ~17s | `CONTAINER-TESTING-GUIDE.md` |

---

## 🚀 Getting Started

### In VM (Now):
1. Read `Apha.BatchJobs.Worker\VM-TESTING-GUIDE.md`
2. Run tests using `dotnet run`
3. ✅ Both NoDb and WithDb work

### In GitHub Codespaces (When Needed):
1. Open GitHub repository: https://github.com/DEFRA/apha-fps-apps
2. Click "Code" → "Codespaces" → "Create codespace on A-Foundation"
3. Wait for Codespace to start
4. Read `CONTAINER-TESTING-GUIDE.md`
5. Run tests using `docker-compose`
6. ✅ Both NoDb and WithDb work

---

## 📞 Need Help?

**If in VM:**
- Check: `Apha.BatchJobs.Worker\VM-TESTING-GUIDE.md`
- Quick ref: `Apha.BatchJobs.Worker\README.md`
- Security: `Apha.BatchJobs.Worker\SECURITY-BEST-PRACTICES.md`

**If in Codespaces:**
- Check: `CONTAINER-TESTING-GUIDE.md`
- Docker docs: https://docs.docker.com/compose/

---

## 🔑 Key Principle

**Same Application, Different Runtime Environments:**

- **VM** = Direct .NET execution (like production server)
- **Codespaces** = Containerized execution (like cloud deployment)

Both use the same configuration pattern:
- `ASPNETCORE_ENVIRONMENT=Demo` → NoDb (in-memory)
- `ASPNETCORE_ENVIRONMENT=Development` → WithDb (PostgreSQL)

**Simple. No overcomplications.** ✅
