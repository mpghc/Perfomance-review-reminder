# Performance Review Reminder Bot

An automated GitHub Action that creates reminder issues for conducting performance reviews.

## Overview

This bot automatically creates GitHub issues as reminders for performance reviews on a monthly schedule. Each issue includes a checklist of actions required and relevant timeline information.

## How It Works

The bot runs as a GitHub Action on the first Monday of each month at 9 AM UTC. When triggered, it:

1. Creates a new GitHub issue with a descriptive title including the current month and year
2. Adds a checklist of performance review tasks
3. Labels the issue with `reminder` and `performance-review` tags
4. Includes timeline information for the review period

## Schedule

- **Automatic Runs:** First Monday of each month at 9:00 AM UTC
- **Manual Trigger:** Can be manually triggered from the Actions tab

## Configuration

The workflow file is located at `.github/workflows/performance-review-reminder.yml`.

To customize the bot:

1. **Change Schedule:** Modify the cron expression in the workflow file
2. **Customize Issue Content:** Edit the title and body templates in the workflow script
3. **Add Assignees:** Add assignees to the `github.rest.issues.create()` call
4. **Modify Labels:** Change the labels array to use custom labels

## Permissions

The workflow requires the following permissions:
- `issues: write` - To create new issues
- `contents: read` - To read repository contents

## Usage

### Manual Trigger

To manually trigger a reminder issue:

1. Go to the **Actions** tab in your repository
2. Select **Performance Review Reminder** workflow
3. Click **Run workflow**
4. Click **Run workflow** button in the dropdown

### Automated Runs

The workflow will automatically run on the configured schedule without any manual intervention.

## Example Issue

When triggered, the bot creates an issue like:

```
Title: 📝 Performance Review Reminder - February 2026

Body:
## Performance Review Reminder

This is an automated reminder to conduct performance reviews for this period.

### Actions Required:
- [ ] Schedule performance review meetings
- [ ] Review employee goals and achievements
- [ ] Prepare feedback and evaluations
- [ ] Document review outcomes
- [ ] Set goals for next review period

### Timeline:
- **Review Period:** February 2026
- **Due Date:** End of month
```

## License

This project is open source and available under standard GitHub terms.
