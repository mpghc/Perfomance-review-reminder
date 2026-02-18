# Iteration 6: Feedback Submission & Tracking

**Labels:** `iteration`, `ui`, `feedback`, `business-logic`
**Priority:** High
**Estimated Time:** 1-2 sessions
**Depends on:** Iteration 5

## Goal
Implement feedback submission functionality and tracking for performance reviews.

## Tasks

### Feedback Pages
- [ ] Create FeedbackSubmission.razor page (@layout MainLayout)
  - Review information display (read-only)
  - Employee being reviewed (read-only)
  - Textarea for feedback content (required, min 50 chars)
  - Character counter
  - Submit and Cancel buttons
  - Validation
- [ ] Create MyFeedbacks.razor page (@layout MainLayout)
  - List of feedbacks submitted by current user
  - Filter by date, review status
  - View submitted feedback content
  - Link to review details

### Feedback Components
- [ ] Create FeedbackList.razor component
  - Display list of feedbacks for a review
  - Show: Author name, Submission date, Content preview
  - Expand/collapse for full content
  - Indicate missing feedbacks with badges
  - Parameter: ReviewId (int)
- [ ] Create FeedbackCard.razor component
  - Display individual feedback
  - Author information
  - Submission timestamp
  - Full content
  - Status indicator
- [ ] Create MissingFeedbackIndicator.razor component
  - Show list of expected reviewers
  - Highlight who has/hasn't submitted
  - Color-coded badges (green: submitted, red: missing)

### Service Updates
- [ ] Update FeedbackService with additional methods:
  - `Task<List<Employee>> GetExpectedReviewersAsync(int reviewId)`
  - `Task<Dictionary<int, bool>> GetFeedbackStatusMapAsync(int reviewId)`
  - `Task<int> GetMissingFeedbackCountAsync(int reviewId)`
- [ ] Update ReviewService:
  - `Task AutoUpdateReviewStatusAsync(int reviewId)` - Update to Completed when all feedbacks received

### Update Existing Pages
- [ ] Update ReviewDetails.razor
  - Add FeedbackList component
  - Add MissingFeedbackIndicator component
  - Show feedback statistics (X of Y submitted)
  - Add "Submit Feedback" button (if user is expected reviewer and hasn't submitted)
  - Auto-refresh status after feedback submission

## Feedback Submission Logic
1. User navigates to review details
2. If user is expected reviewer and hasn't submitted:
   - Show "Submit Feedback" button
   - Button navigates to FeedbackSubmission.razor?reviewId={id}
3. User fills out feedback form
4. On submit:
   - Validate feedback (required, min length)
   - Check for duplicates (prevent user from submitting twice)
   - Save feedback to database
   - Update review status if all feedbacks collected
   - Show success message
   - Navigate back to review details

## Validation Rules
- [ ] Feedback content: Required, MinLength(50), MaxLength(5000)
- [ ] Review must exist and be in valid status (not Cancelled)
- [ ] User must be expected reviewer for the review
- [ ] User cannot submit feedback twice for same review
- [ ] Feedback cannot be submitted for completed reviews (optional rule)

## UI Components Example

### FeedbackSubmission.razor
```razor
@page "/feedback/submit/{reviewId:int}"
@layout MainLayout
@inject IFeedbackService FeedbackService
@inject IReviewService ReviewService
@inject NavigationManager NavigationManager

<h3>Submit Feedback</h3>

@if (review != null)
{
    <div class="card mb-3">
        <div class="card-body">
            <h5>Performance Review Details</h5>
            <p><strong>Employee:</strong> @review.Employee?.Name</p>
            <p><strong>Scheduled Date:</strong> @review.ScheduledDate.ToShortDateString()</p>
            <p><strong>Status:</strong> <StatusBadge Status="@review.Status" /></p>
        </div>
    </div>

    <EditForm Model="feedback" OnValidSubmit="SubmitFeedback">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <div class="mb-3">
            <label class="form-label">Feedback *</label>
            <InputTextArea class="form-control" rows="10" @bind-Value="feedback.Content" />
            <div class="form-text">Minimum 50 characters. @feedback.Content.Length / 5000</div>
            <ValidationMessage For="@(() => feedback.Content)" />
        </div>

        <button type="submit" class="btn btn-primary" disabled="@isSubmitting">
            @(isSubmitting ? "Submitting..." : "Submit Feedback")
        </button>
        <button type="button" class="btn btn-secondary" @onclick="Cancel">Cancel</button>
    </EditForm>
}
```

### MissingFeedbackIndicator.razor
```razor
@inject IFeedbackService FeedbackService
@inject IEmployeeService EmployeeService

<div class="card">
    <div class="card-header">
        <h5>Feedback Status</h5>
    </div>
    <div class="card-body">
        <p><strong>@submittedCount of @expectedCount</strong> feedbacks submitted</p>
        
        <h6>Expected Reviewers:</h6>
        <ul class="list-unstyled">
            @foreach (var reviewer in expectedReviewers)
            {
                <li>
                    @if (feedbackStatusMap.ContainsKey(reviewer.Id) && feedbackStatusMap[reviewer.Id])
                    {
                        <span class="badge bg-success">✓</span>
                    }
                    else
                    {
                        <span class="badge bg-danger">✗</span>
                    }
                    @reviewer.Name
                </li>
            }
        </ul>
    </div>
</div>
```

## Acceptance Criteria
- [ ] Users can submit feedback for reviews they're assigned to
- [ ] Feedback form validates inputs properly
- [ ] Duplicate submissions are prevented
- [ ] Feedback list shows all submissions for a review
- [ ] Missing feedbacks are clearly indicated
- [ ] Review status auto-updates when all feedbacks received
- [ ] Character counter works in real-time
- [ ] Success message shown after submission
- [ ] Error handling for failed submissions
- [ ] Navigation works properly between pages
- [ ] UI is responsive and user-friendly

## Testing Checklist
- [ ] Submit feedback as assigned reviewer
- [ ] Try to submit duplicate feedback (should fail)
- [ ] Try to submit feedback for review not assigned to (should fail)
- [ ] View feedback list on review details page
- [ ] Verify missing feedback indicator is accurate
- [ ] Verify review status changes to Completed when all feedbacks submitted
- [ ] Test character counter updates
- [ ] Test validation messages display
- [ ] Test cancel button returns to previous page
- [ ] Test form submission with minimum character requirement

## Dependencies
- Iteration 5 must be completed

## Notes
- Consider adding rich text editor for feedback (future enhancement)
- May want to add email notifications when feedback submitted (future)
- Keep feedback submission simple and user-friendly
- Auto-save draft functionality could be added later
- Human reviews feedback flow before implementation
