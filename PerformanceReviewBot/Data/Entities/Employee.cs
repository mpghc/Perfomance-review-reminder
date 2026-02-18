namespace PerformanceReviewBot.Data.Entities;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsManager { get; set; }
    public int? ManagerId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
    public ICollection<Feedback> FeedbacksGiven { get; set; } = new List<Feedback>();
    public ICollection<ReminderLog> ReminderLogs { get; set; } = new List<ReminderLog>();

    public string FullName => $"{FirstName} {LastName}";
}
