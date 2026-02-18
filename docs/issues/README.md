# GitHub Issues for Project Stages

This directory contains detailed issue templates for the 8 stages of the Performance Review Reminder Bot project.

## 📋 Available Issue Templates

1. **Stage 1: Solution Structure** - Set up .NET solution and projects
2. **Stage 2: Folder Organization** - Define folder structure and architecture
3. **Stage 3: Entities and Relationships** - Create domain models and DbContext
4. **Stage 4: Services Definition** - Implement business logic layer
5. **Stage 5: Pages and Routing** - Build Blazor pages and navigation
6. **Stage 6: Layout Strategy** - Create layouts and reusable components
7. **Stage 7: Reminder Flow** - Implement reminder logic and background service
8. **Stage 8: Testing Strategy** - Write comprehensive unit tests

## 🚀 Quick Start: Create All Issues Automatically

### Prerequisites
- [GitHub CLI (`gh`)](https://cli.github.com/) installed
- Authenticated with GitHub: `gh auth login`

### Create All Issues at Once

From the repository root, run:

```bash
./create-issues.sh
```

This script will:
- ✅ Verify GitHub CLI is installed and authenticated
- ✅ Create all 8 issues in the correct order
- ✅ Apply labels: `enhancement`, `project-stage`
- ✅ Use the detailed markdown templates as issue bodies
- ✅ Provide URLs to the created issues

### Expected Output

```
Creating GitHub issues for Performance Review Reminder Bot stages...
Repository: mpghc/Perfomance-review-reminder

✅ GitHub CLI is installed and authenticated

Creating issue: Stage 1: Solution Structure
✅ Created: https://github.com/mpghc/Perfomance-review-reminder/issues/1

Creating issue: Stage 2: Folder Organization
✅ Created: https://github.com/mpghc/Perfomance-review-reminder/issues/2

...

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Successfully created 8 issues
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## 📝 Manual Issue Creation (Alternative)

If you prefer to create issues manually or the script doesn't work for you:

1. Go to [GitHub Issues](https://github.com/mpghc/Perfomance-review-reminder/issues/new)
2. Copy the content from the corresponding markdown file in `docs/issues/`
3. Paste as the issue body
4. Set the title (e.g., "Stage 1: Solution Structure")
5. Add labels: `enhancement`, `project-stage`
6. Submit the issue

### Example: Create Stage 1 Issue Manually

```bash
# View the content
cat docs/issues/stage-1-solution-structure.md

# Or create via gh CLI individually
gh issue create \
  --title "Stage 1: Solution Structure" \
  --body-file docs/issues/stage-1-solution-structure.md \
  --label "enhancement,project-stage"
```

## 🏗️ Stage Dependencies

Issues should be completed in order due to dependencies:

```
Stage 1 (Solution Structure)
    ↓
Stage 2 (Folder Organization)
    ↓
Stage 3 (Entities and Relationships)
    ↓
Stage 4 (Services Definition)
    ↓  ↘
Stage 5 (Pages)   Stage 7 (Reminder Flow)
    ↓         ↙
Stage 6 (Layouts)
    ↓
Stage 8 (Testing Strategy)
```

**Note:** Some stages can be worked on in parallel after their dependencies are met.

## 📊 Progress Tracking

Track progress by:
- ✅ Marking tasks in issue checklists as complete
- 🏷️ Adding status labels (in-progress, blocked, review, done)
- 🎯 Using GitHub Projects board
- 📈 Monitoring via GitHub Insights

## 🔧 Customization

### Modify Issue Templates

Edit the markdown files in `docs/issues/` to:
- Add/remove tasks
- Adjust acceptance criteria
- Update effort estimates
- Include additional technical notes

### Modify the Script

Edit `create-issues.sh` to:
- Change label names
- Add milestone assignment
- Add assignee assignment
- Modify issue creation logic

Example - Add milestone:
```bash
gh issue create \
  --repo "$REPO" \
  --title "$title" \
  --body-file "$filepath" \
  --label "$LABELS" \
  --milestone "v1.0"
```

## 🆘 Troubleshooting

### GitHub CLI Not Installed
```bash
# macOS
brew install gh

# Windows
winget install --id GitHub.cli

# Linux
# See: https://github.com/cli/cli/blob/trunk/docs/install_linux.md
```

### Not Authenticated
```bash
gh auth login
# Follow the prompts to authenticate
```

### Script Permission Denied
```bash
chmod +x create-issues.sh
```

### Rate Limit Errors
The script includes a 1-second delay between issues. If you still hit rate limits:
- Wait a few minutes
- Run the script again (it will skip already created issues if you modify it)

## 📚 Additional Resources

- [GitHub CLI Documentation](https://cli.github.com/manual/)
- [GitHub Issues Guide](https://docs.github.com/en/issues)
- [Project README](../README.md)
- [C# Coding Instructions](../.github/instructions/csharp.instructions.md)
- [Blazor Coding Instructions](../.github/instructions/blazor.instructions.md)

## 💡 Tips

1. **Create issues in batches** - Run the script to create all at once
2. **Review before starting** - Read through all issues to understand the full scope
3. **Update as you go** - Check off tasks as you complete them
4. **Link PRs to issues** - Use "Closes #X" in PR descriptions
5. **Ask questions** - Comment on issues if clarification is needed

## 📞 Support

If you encounter issues or have questions:
1. Check existing GitHub Issues
2. Review the troubleshooting section above
3. Create a new issue with the `question` label
4. Contact the project maintainer

---

**Ready to start?** Run `./create-issues.sh` from the repository root! 🚀
