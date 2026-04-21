# Security Best Practices - MUST READ

## ⚠️ NEVER Commit Passwords or Secrets to Git

### What NOT to Commit:
- ❌ Passwords
- ❌ API keys
- ❌ Connection strings with credentials
- ❌ Certificates or private keys
- ❌ OAuth tokens
- ❌ Any sensitive configuration

### What TO Commit:
- ✅ `appsettings.json` (base configuration without secrets)
- ✅ `appsettings.Development.json` (structure only, no secrets)
- ✅ `appsettings.Production.json` (structure only, no secrets)
- ✅ `appsettings.local.json.example` (template with placeholders)

### What NOT to Commit (Already in .gitignore):
- ✅ `appsettings.local.json` (contains real secrets)
- ✅ `appsettings.*.local.json`
- ✅ `*.user` files
- ✅ `secrets.json`

---

## ✅ Correct Pattern for Local Development

### 1. Use appsettings.local.json for Secrets

**File**: `appsettings.local.json` (NOT committed to Git)
```json
{
  "ConnectionStrings": {
    "BatchJobsConnectionString": "Host=localhost;Port=5432;Database=batchjobs;Username=postgres;Password=YOUR_REAL_PASSWORD"
  }
}
```

### 2. Use Example Files as Templates

**File**: `appsettings.local.json.example` (committed to Git)
```json
{
  "ConnectionStrings": {
    "BatchJobsConnectionString": "Host=localhost;Port=5432;Database=batchjobs;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
  }
}
```

### 3. Keep appsettings.Development.json Clean

**File**: `appsettings.Development.json` (committed to Git)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
  // NO CONNECTION STRINGS HERE!
}
```

---

## 🔧 Setup for New Developers

When a new developer clones the repository:

```powershell
# Navigate to Worker directory
cd src\Apha.BatchJobs\Apha.BatchJobs.Worker

# Copy the example file
Copy-Item appsettings.local.json.example appsettings.local.json

# Edit appsettings.local.json with your actual credentials
# (This file will NOT be committed to Git)
```

---

## 🚨 What to Do If You Accidentally Commit Secrets

### Immediate Actions:

1. **DO NOT PUSH** if you haven't pushed yet
   ```powershell
   git reset --soft HEAD~1  # Undo the commit, keep changes
   # Remove the secrets from files
   git add .
   git commit -m "Remove secrets"
   ```

2. **If Already Pushed**: 
   - Immediately change/rotate all exposed credentials
   - Contact your team lead
   - Follow the "Remove from Git History" steps below

3. **Remove from Git History** (requires force push):
   ```powershell
   # See SECURITY-GIT-HISTORY-CLEANUP.md for detailed steps
   ```

---

## 🔐 Alternative: Use User Secrets (Development)

For ASP.NET Core projects, use the Secret Manager tool:

```powershell
# Initialize user secrets
dotnet user-secrets init --project YourProject.csproj

# Set a secret
dotnet user-secrets set "ConnectionStrings:BatchJobsConnectionString" "Host=localhost;..."

# Secrets are stored outside the project folder and NOT in Git
```

---

## 🌐 Production Deployments

### For Production/Staging Environments:

Use environment-specific configuration sources (in priority order):

1. **Environment Variables** (highest priority)
   ```powershell
   $env:ConnectionStrings__BatchJobsConnectionString = "Host=prod-server;..."
   ```

2. **Azure Key Vault** (recommended for Azure deployments)
   ```csharp
   builder.Configuration.AddAzureKeyVault(...);
   ```

3. **AWS Secrets Manager** (for AWS deployments)

4. **HashiCorp Vault** (for on-premise)

5. **appsettings.local.json** (ONLY for local dev, never deploy this file)

---

## ✅ Pre-Commit Checklist

Before committing, always check:

- [ ] Run `git diff` and review all changes
- [ ] Search for passwords: `git diff | Select-String "Password"`
- [ ] Search for connection strings: `git diff | Select-String "ConnectionString"`
- [ ] Verify no `appsettings.local.json` in staged files: `git status`
- [ ] No API keys, tokens, or secrets in the diff

---

## 📋 Quick Commands

```powershell
# Check what's staged for commit
git status

# See the actual changes
git diff --cached

# Check if any secrets are staged
git diff --cached | Select-String -Pattern "password|secret|key|token" -CaseSensitive:$false

# Verify .gitignore is working
git check-ignore appsettings.local.json
# Should output: appsettings.local.json
```

---

## 🎓 Training Resources

- [OWASP Secrets Management Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)
- [GitHub: Removing sensitive data from a repository](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/removing-sensitive-data-from-a-repository)
- [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

---

## 🚫 Common Mistakes to Avoid

| ❌ Wrong | ✅ Right |
|---------|---------|
| Committing passwords in `appsettings.Development.json` | Use `appsettings.local.json` |
| Hardcoding connection strings | Use configuration or environment variables |
| Sharing credentials via email/chat | Use secure secret management tools |
| Same passwords in dev and prod | Use different credentials per environment |
| Committing `.env` files | Add `.env` to `.gitignore` |

---

## 📝 Code Review Checklist

When reviewing pull requests:

- [ ] No passwords or secrets in the diff
- [ ] Connection strings use placeholders or are removed
- [ ] No hardcoded credentials in code
- [ ] Sensitive files are in `.gitignore`
- [ ] Example/template files don't contain real secrets

---

**Remember**: Once something is committed to Git, it's in the history forever unless explicitly removed. Always be careful!

**Questions?** Contact your security team or team lead.
