namespace PerformanceReviewBot.Data.Entities;

public class PerformanceReview
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime ReviewDate { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Scheduled;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }

    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<ReminderLog> ReminderLogs { get; set; } = new List<ReminderLog>();
}
