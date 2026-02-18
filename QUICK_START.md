# Quick Reference: Creating GitHub Issues

## 🎯 Goal
Create 8 GitHub issues for the Performance Review Reminder Bot project stages.

## ⚡ Quick Start (One Command)

```bash
./create-issues.sh
```

That's it! This will create all 8 issues automatically.

## 📋 What Gets Created

| Issue # | Title | Size | Time | Tasks |
|---------|-------|------|------|-------|
| 1 | Stage 1: Solution Structure | Small | 0.5-1h | 8 |
| 2 | Stage 2: Folder Organization | Small | 0.25-0.5h | 11 |
| 3 | Stage 3: Entities and Relationships | Medium | 2-3h | 13 |
| 4 | Stage 4: Services Definition | Large | 4-6h | 17 |
| 5 | Stage 5: Pages and Routing | Large | 6-8h | 18 |
| 6 | Stage 6: Layout Strategy | Medium | 3-4h | 14 |
| 7 | Stage 7: Reminder Flow | Med-Large | 4-5h | 10 |
| 8 | Stage 8: Testing Strategy | Med-Large | 4-6h | 14 |

**Total:** 8 issues, 105+ tasks, 20-30 hours estimated

## 🔧 Prerequisites

### Install GitHub CLI

**macOS:**
```bash
brew install gh
```

**Windows:**
```bash
winget install --id GitHub.cli
```

**Linux:**
```bash
# Debian/Ubuntu
sudo apt install gh

# Red Hat/Fedora
sudo dnf install gh

# Or download from: https://cli.github.com/
```

### Authenticate

```bash
gh auth login
```

Follow the prompts to authenticate with GitHub.

## 🚀 Alternative Methods

### Method 1: Automated (Recommended)
```bash
./create-issues.sh
```

### Method 2: Individual Issue via CLI
```bash
gh issue create \
  --title "Stage 1: Solution Structure" \
  --body-file docs/issues/stage-1-solution-structure.md \
  --label "enhancement,project-stage"
```

### Method 3: Manual via GitHub Web UI
1. Go to https://github.com/mpghc/Perfomance-review-reminder/issues/new
2. Open `docs/issues/stage-1-solution-structure.md`
3. Copy the content
4. Paste into issue body
5. Set title: "Stage 1: Solution Structure"
6. Add labels: `enhancement`, `project-stage`
7. Click "Submit new issue"
8. Repeat for stages 2-8

## 📖 Documentation Locations

- **Script:** `./create-issues.sh`
- **Templates:** `docs/issues/stage-*.md`
- **Guide:** `docs/issues/README.md`
- **Main README:** `README.md`

## ✅ Verification

After running the script, verify:

```bash
# Check if all 8 issues were created
gh issue list --label "project-stage"

# Should show 8 issues
```

## 🔗 Useful Commands

```bash
# List all issues
gh issue list

# View a specific issue
gh issue view 1

# Close an issue
gh issue close 1

# Reopen an issue
gh issue reopen 1

# Add a label
gh issue edit 1 --add-label "in-progress"

# Assign to yourself
gh issue edit 1 --add-assignee @me
```

## 📊 Project Flow

```
Start → Install gh CLI → Authenticate → Run Script → Review Issues → Start Work
```

## 🆘 Troubleshooting

### Script says "gh not found"
→ Install GitHub CLI (see Prerequisites above)

### Script says "not authenticated"
→ Run `gh auth login`

### Permission denied on script
→ Run `chmod +x create-issues.sh`

### Issues already exist
→ Script will fail on duplicates. Delete existing issues or skip.

### Want to modify templates
→ Edit files in `docs/issues/` before running script

## 📝 Tips

1. **Read first** - Review all templates before creating issues
2. **Customize** - Edit templates if you need different tasks
3. **Test locally** - Make changes in a test repo first
4. **Track progress** - Check off tasks as you complete them
5. **Link PRs** - Use "Closes #X" in PR descriptions

## 🎓 What Each Template Contains

Every stage template includes:
- ✅ Clear objective
- ✅ Detailed description
- ✅ Complete task checklist
- ✅ Acceptance criteria
- ✅ Technical notes
- ✅ Dependencies
- ✅ Effort estimate
- ✅ Architecture guidelines

## 🔐 Security Note

The script only creates issues. It does not:
- Delete anything
- Modify existing code
- Access sensitive data
- Make any destructive changes

Safe to run!

## 📞 Need Help?

- **Documentation:** See `docs/issues/README.md`
- **GitHub CLI Help:** `gh issue create --help`
- **Project Issues:** https://github.com/mpghc/Perfomance-review-reminder/issues
- **GitHub CLI Docs:** https://cli.github.com/manual/

---

**Ready?** Run `./create-issues.sh` now! 🚀
