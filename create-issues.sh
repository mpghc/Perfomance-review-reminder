#!/bin/bash

# Script to create GitHub issues for Performance Review Reminder Bot stages
# Requires: GitHub CLI (gh) to be installed and authenticated
# Usage: ./create-issues.sh

set -e

REPO="mpghc/Perfomance-review-reminder"
DOCS_DIR="docs/issues"

echo "Creating GitHub issues for Performance Review Reminder Bot stages..."
echo "Repository: $REPO"
echo ""

# Check if gh CLI is installed
if ! command -v gh &> /dev/null; then
    echo "❌ Error: GitHub CLI (gh) is not installed."
    echo "Please install it from: https://cli.github.com/"
    exit 1
fi

# Check if authenticated
if ! gh auth status &> /dev/null; then
    echo "❌ Error: Not authenticated with GitHub CLI."
    echo "Please run: gh auth login"
    exit 1
fi

echo "✅ GitHub CLI is installed and authenticated"
echo ""

# Array of stage files and their titles
declare -a stages=(
    "stage-1-solution-structure.md:Stage 1: Solution Structure"
    "stage-2-folder-organization.md:Stage 2: Folder Organization"
    "stage-3-entities-relationships.md:Stage 3: Entities and Relationships"
    "stage-4-services-definition.md:Stage 4: Services Definition"
    "stage-5-pages-routing.md:Stage 5: Pages and Routing"
    "stage-6-layout-strategy.md:Stage 6: Layout Strategy"
    "stage-7-reminder-flow.md:Stage 7: Reminder Flow"
    "stage-8-testing-strategy.md:Stage 8: Testing Strategy"
)

# Labels to apply to all issues
LABELS="enhancement,project-stage"

# Create each issue
issue_count=0
for stage_info in "${stages[@]}"; do
    IFS=':' read -r filename title <<< "$stage_info"
    filepath="$DOCS_DIR/$filename"
    
    if [ ! -f "$filepath" ]; then
        echo "⚠️  Warning: File not found: $filepath"
        continue
    fi
    
    echo "Creating issue: $title"
    
    # Create the issue and capture the URL
    issue_url=$(gh issue create \
        --repo "$REPO" \
        --title "$title" \
        --body-file "$filepath" \
        --label "$LABELS")
    
    if [ $? -eq 0 ]; then
        echo "✅ Created: $issue_url"
        ((issue_count++))
    else
        echo "❌ Failed to create issue: $title"
    fi
    
    echo ""
    
    # Small delay to avoid rate limiting
    sleep 1
done

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Successfully created $issue_count issues"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "View all issues at: https://github.com/$REPO/issues"
echo ""
echo "Next steps:"
echo "1. Review the created issues"
echo "2. Assign issues to team members"
echo "3. Add additional labels or milestones as needed"
echo "4. Start working on Stage 1: Solution Structure"
