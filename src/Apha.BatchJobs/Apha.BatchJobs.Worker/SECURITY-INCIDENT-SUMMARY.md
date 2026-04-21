# ⚠️ SECURITY INCIDENT - ACTION REQUIRED

## What Happened
Passwords were accidentally committed to Git history in the following files:
- `src/Apha.BatchJobs/Apha.BatchJobs.Worker/appsettings.Development.json`
- `src/Apha.BatchJobs/Apha.BatchJobs.Worker/appsettings.Production.json`

**Commits affected**: efad9de through 9241779 (multiple commits on A-Foundation branch)

## Immediate Actions Taken ✅
1. ✅ Removed passwords from current files (commit 9241779)
2. ✅ Created `appsettings.local.json.example` as template
3. ✅ Updated documentation
4. ✅ Created security best practices guide

## ⚠️ Actions Still Needed

### 1. Clean Git History (Team Lead Required)
The passwords still exist in git history and need to be removed.

**Option A: Run the automated script**
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
.\cleanup-git-history.ps1
```

**Option B: Manual cleanup**
See `SECURITY-GIT-HISTORY-CLEANUP.md` for detailed instructions.

### 2. Rotate Credentials Immediately 🔐
Change the following passwords/credentials:
- [ ] PostgreSQL `postgres` user password (localhost)
- [ ] Production database password (if exposed)
- [ ] Any other credentials in those files

### 3. Team Coordination
After history cleanup:
- Notify all team members
- Have them re-sync: `git fetch origin && git reset --hard origin/A-Foundation`

## 📚 Documentation Created

1. **SECURITY-BEST-PRACTICES.md** - Prevent future incidents
   - What NOT to commit
   - How to use `appsettings.local.json`
   - Pre-commit checklist

2. **SECURITY-GIT-HISTORY-CLEANUP.md** - Remove secrets from history
   - Step-by-step guide
   - Multiple cleanup methods
   - Team coordination steps

3. **cleanup-git-history.ps1** - Automated cleanup script
   - Creates backup automatically
   - Rewrites history
   - Provides next steps

## ✅ Going Forward

**Always use `appsettings.local.json` for secrets:**

1. Copy the example:
   ```powershell
   Copy-Item appsettings.local.json.example appsettings.local.json
   ```

2. Add your real credentials to `appsettings.local.json`

3. **NEVER commit `appsettings.local.json`** (it's in .gitignore)

## 🔍 Verify Current Status

```powershell
# Check if secrets are in current files (should be none)
git grep -i "password=" -- "*.json"

# Check if secrets are in history (will show old commits)
git log -S "Password=postgres" --all --oneline
```

## 📞 Questions?
- Review `SECURITY-BEST-PRACTICES.md`
- Contact your security team
- Refer to `SECURITY-GIT-HISTORY-CLEANUP.md` for history cleanup

---

**Status**: Documentation complete ✅ | History cleanup pending ⚠️ | Credentials rotation pending ⚠️
