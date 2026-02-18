namespace PerformanceReviewBot.Data.Entities;

public class Feedback
{
    public int Id { get; set; }
    public int PerformanceReviewId { get; set; }
    public int ReviewerId { get; set; }
    public string Comments { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public bool IsManagerFeedback { get; set; }

    // Navigation properties
    public PerformanceReview PerformanceReview { get; set; } = null!;
    public Employee Reviewer { get; set; } = null!;
}
