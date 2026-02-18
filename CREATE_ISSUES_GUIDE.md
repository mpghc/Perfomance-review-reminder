# How to Create GitHub Issues from Templates

Since automated issue creation requires special permissions, please create the GitHub issues manually using the templates provided in the `.github/ISSUE_TEMPLATES/` directory.

## Quick Instructions

1. Go to your repository: https://github.com/mpghc/Perfomance-review-reminder
2. Click on "Issues" tab
3. Click "New Issue" button
4. For each template file in `.github/ISSUE_TEMPLATES/`, create a new issue:

### Iteration 1: Project Setup & Infrastructure
- **File:** `iteration-01-project-setup.md`
- **Labels:** `iteration`, `setup`, `infrastructure`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 2: Core Domain Entities & Database
- **File:** `iteration-02-domain-entities.md`
- **Labels:** `iteration`, `database`, `entities`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 3: Service Layer Implementation
- **File:** `iteration-03-service-layer.md`
- **Labels:** `iteration`, `services`, `business-logic`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 4: Basic Blazor UI & Layouts
- **File:** `iteration-04-blazor-ui-layouts.md`
- **Labels:** `iteration`, `ui`, `blazor`, `layouts`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 5: Employee & Review Management Pages
- **File:** `iteration-05-crud-pages.md`
- **Labels:** `iteration`, `ui`, `blazor`, `crud`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 6: Feedback Submission & Tracking
- **File:** `iteration-06-feedback-submission.md`
- **Labels:** `iteration`, `ui`, `feedback`, `business-logic`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 7: Reminder Service & Background Worker
- **File:** `iteration-07-reminder-service.md`
- **Labels:** `iteration`, `services`, `background-service`, `critical`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 8: Admin Reporting & Dashboard
- **File:** `iteration-08-admin-reporting.md`
- **Labels:** `iteration`, `ui`, `admin`, `reporting`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 9: Testing & Quality Assurance
- **File:** `iteration-09-testing.md`
- **Labels:** `iteration`, `testing`, `quality`
- **Title:** Copy from file
- **Body:** Copy content from file

### Iteration 10: Final Integration & Documentation
- **File:** `iteration-10-final-integration.md`
- **Labels:** `iteration`, `documentation`, `deployment`, `final`
- **Title:** Copy from file
- **Body:** Copy content from file

## Alternative: Use GitHub CLI (if you have it configured)

If you have GitHub CLI (`gh`) installed and authenticated, you can run:

```bash
cd /home/runner/work/Perfomance-review-reminder/Perfomance-review-reminder

# Iteration 1
gh issue create --title "Iteration 1: Project Setup & Infrastructure" \
  --body-file .github/ISSUE_TEMPLATES/iteration-01-project-setup.md \
  --label "iteration,setup,infrastructure"

# Iteration 2
gh issue create --title "Iteration 2: Core Domain Entities & Database" \
  --body-file .github/ISSUE_TEMPLATES/iteration-02-domain-entities.md \
  --label "iteration,database,entities"

# Iteration 3
gh issue create --title "Iteration 3: Service Layer Implementation" \
  --body-file .github/ISSUE_TEMPLATES/iteration-03-service-layer.md \
  --label "iteration,services,business-logic"

# Iteration 4
gh issue create --title "Iteration 4: Basic Blazor UI & Layouts" \
  --body-file .github/ISSUE_TEMPLATES/iteration-04-blazor-ui-layouts.md \
  --label "iteration,ui,blazor,layouts"

# Iteration 5
gh issue create --title "Iteration 5: Employee & Review Management Pages" \
  --body-file .github/ISSUE_TEMPLATES/iteration-05-crud-pages.md \
  --label "iteration,ui,blazor,crud"

# Iteration 6
gh issue create --title "Iteration 6: Feedback Submission & Tracking" \
  --body-file .github/ISSUE_TEMPLATES/iteration-06-feedback-submission.md \
  --label "iteration,ui,feedback,business-logic"

# Iteration 7
gh issue create --title "Iteration 7: Reminder Service & Background Worker" \
  --body-file .github/ISSUE_TEMPLATES/iteration-07-reminder-service.md \
  --label "iteration,services,background-service,critical"

# Iteration 8
gh issue create --title "Iteration 8: Admin Reporting & Dashboard" \
  --body-file .github/ISSUE_TEMPLATES/iteration-08-admin-reporting.md \
  --label "iteration,ui,admin,reporting"

# Iteration 9
gh issue create --title "Iteration 9: Testing & Quality Assurance" \
  --body-file .github/ISSUE_TEMPLATES/iteration-09-testing.md \
  --label "iteration,testing,quality"

# Iteration 10
gh issue create --title "Iteration 10: Final Integration & Documentation" \
  --body-file .github/ISSUE_TEMPLATES/iteration-10-final-integration.md \
  --label "iteration,documentation,deployment,final"
```

## Labels to Create First

If these labels don't exist in your repository, create them first:
- `iteration` - For all iteration issues
- `setup` - Infrastructure and setup
- `database` - Database related
- `entities` - Domain entities
- `services` - Service layer
- `business-logic` - Business logic
- `ui` - User interface
- `blazor` - Blazor specific
- `layouts` - Layout related
- `crud` - CRUD operations
- `feedback` - Feedback functionality
- `background-service` - Background services
- `critical` - Critical functionality
- `admin` - Admin features
- `reporting` - Reporting features
- `testing` - Testing related
- `quality` - Quality assurance
- `documentation` - Documentation
- `deployment` - Deployment related
- `final` - Final iteration

## Next Steps

After creating all issues:
1. Review the ITERATIONS.md file for the complete plan
2. Start with Iteration 1
3. Wait for human approval before proceeding to each next iteration
4. Follow the AI-assisted development process outlined in ITERATIONS.md
