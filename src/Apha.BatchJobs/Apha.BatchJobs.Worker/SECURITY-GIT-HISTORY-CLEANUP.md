# Removing Secrets from Git History

## ⚠️ WARNING: This Rewrites Git History

This guide helps remove committed secrets from Git history. **This requires force pushing and coordination with your team.**

---

## 🔍 Check If Secrets Are in History

```powershell
# Search for password in all commits
git log -S "Password=postgres" --all --source --full-history

# Search in specific file history
git log --all --full-history -- "Apha.BatchJobs.Worker/appsettings.Development.json"
```

---

## 🛠️ Method 1: Using git filter-repo (Recommended)

### Prerequisites:
```powershell
pip install git-filter-repo
```

### Steps:

1. **Backup your repository first!**
   ```powershell
   cd D:\Users\atos.user8\source\repos
   Copy-Item -Recurse apha-fps-apps-A-Foundation apha-fps-apps-A-Foundation-BACKUP
   ```

2. **Create a replacements file**
   Create `replacements.txt`:
   ```
   Password=postgres==>Password=YOUR_PASSWORD
   ```

3. **Run filter-repo**
   ```powershell
   cd apha-fps-apps-A-Foundation
   git filter-repo --replace-text replacements.txt
   ```

---

## 🛠️ Method 2: Using BFG Repo-Cleaner (Simpler)

### Prerequisites:
Download BFG from: https://rtyley.github.io/bfg-repo-cleaner/

### Steps:

1. **Create a file with passwords to remove**
   Create `passwords.txt`:
   ```
   postgres
   your-production-password
   ```

2. **Run BFG**
   ```powershell
   java -jar bfg.jar --replace-text passwords.txt apha-fps-apps-A-Foundation
   ```

3. **Clean up**
   ```powershell
   cd apha-fps-apps-A-Foundation
   git reflog expire --expire=now --all
   git gc --prune=now --aggressive
   ```

---

## 🛠️ Method 3: Manual Removal (Specific Commits)

If you know exactly which commits contain secrets:

```powershell
# Interactive rebase
git rebase -i HEAD~5  # Adjust number based on how far back

# In the editor, change 'pick' to 'edit' for commits with secrets
# When stopped at each commit:
# 1. Edit the files to remove secrets
# 2. Stage the changes:
git add .
git commit --amend --no-edit
git rebase --continue
```

---

## ⚠️ Force Push (TEAM COORDINATION REQUIRED)

**Before force pushing:**

1. ✅ Notify your entire team
2. ✅ Ensure everyone has pushed their changes
3. ✅ Verify you have a backup

**Force push:**
```powershell
# Force push to remote (overwrites history)
git push origin A-Foundation --force

# Or use force-with-lease (safer)
git push origin A-Foundation --force-with-lease
```

---

## 👥 Team Coordination Steps

### For Team Lead:

1. Announce in team chat:
   ```
   🚨 GIT HISTORY REWRITE ALERT 🚨

   We're removing secrets from git history.

   ACTION REQUIRED by [TIME]:
   1. Push all your pending changes
   2. After the rewrite, follow the "Team Members" steps below

   Branch affected: A-Foundation
   Estimated downtime: 30 minutes
   ```

2. Wait for all team members to push

3. Perform the history rewrite (see methods above)

4. Force push

5. Notify team that rewrite is complete

### For Team Members (After Rewrite):

```powershell
# Save your uncommitted work
git stash

# Fetch the rewritten history
git fetch origin

# Reset your local branch to match remote
git reset --hard origin/A-Foundation

# Restore your work
git stash pop
```

---

## 🔐 Post-Cleanup Actions

After removing secrets from history:

1. ✅ **Rotate all exposed credentials immediately**
   - Database passwords
   - API keys
   - Connection strings
   - Certificates

2. ✅ **Verify removal**
   ```powershell
   git log -S "Password=postgres" --all
   # Should return no results
   ```

3. ✅ **Update documentation**
   - Document the incident
   - Add to security training

4. ✅ **Implement prevention**
   - Add pre-commit hooks
   - Enable secret scanning (GitHub Advanced Security)

---

## 🤖 Automated Secret Scanning

### Enable GitHub Secret Scanning:

1. Go to repository settings
2. Enable "Secret scanning"
3. Enable "Push protection"

### Use git-secrets (Pre-commit Hook):

```powershell
# Install git-secrets
git clone https://github.com/awslabs/git-secrets.git
cd git-secrets
.\install.ps1

# Configure for your repo
cd path\to\your\repo
git secrets --install
git secrets --register-aws
```

---

## 📊 Verify the History is Clean

```powershell
# Check specific file history
git log --all --full-history --stat -- "Apha.BatchJobs.Worker/appsettings.Development.json"

# Search for common secret patterns
git log --all -p | Select-String -Pattern "password\s*=\s*[^Y]" -Context 2

# Check current files don't have secrets
git grep -i "password=" -- "*.json"
```

---

## 🆘 If Something Goes Wrong

### Restore from Backup:

```powershell
# Remove the corrupted repo
cd D:\Users\atos.user8\source\repos
Remove-Item -Recurse -Force apha-fps-apps-A-Foundation

# Restore from backup
Copy-Item -Recurse apha-fps-apps-A-Foundation-BACKUP apha-fps-apps-A-Foundation

# Or re-clone from remote if remote is still good
git clone https://github.com/DEFRA/apha-fps-apps.git apha-fps-apps-A-Foundation
```

---

## 📝 Document the Incident

After successful cleanup, document:

1. What secrets were exposed
2. When they were committed
3. When they were discovered
4. What actions were taken
5. Were credentials rotated?
6. Lessons learned
7. Prevention measures implemented

---

## 🔗 References

- [GitHub: Removing sensitive data](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/removing-sensitive-data-from-a-repository)
- [git-filter-repo documentation](https://github.com/newren/git-filter-repo)
- [BFG Repo-Cleaner](https://rtyley.github.io/bfg-repo-cleaner/)
- [git-secrets](https://github.com/awslabs/git-secrets)

---

**Remember**: Prevention is better than cleanup. Always use `appsettings.local.json` for secrets!
