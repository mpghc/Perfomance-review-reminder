namespace PerformanceReviewBot.Data.Entities;

public class ReminderLog
{
    public int Id { get; set; }
    public int PerformanceReviewId { get; set; }
    public int EmployeeId { get; set; }
    public ReminderType ReminderType { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public PerformanceReview PerformanceReview { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
