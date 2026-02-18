using System.ComponentModel.DataAnnotations;

namespace PerformanceReviewBot.Models;

public class PerformanceReview
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [Display(Name = "Review Date")]
    [DataType(DataType.Date)]
    public DateTime ReviewDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = ReviewStatus.Scheduled;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
}

public static class ReviewStatus
{
    public const string Scheduled = "Scheduled";
    public const string Completed = "Completed";
    public const string Overdue = "Overdue";

    public static readonly string[] All = { Scheduled, Completed, Overdue };
}
