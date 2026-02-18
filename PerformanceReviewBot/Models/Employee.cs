using System.ComponentModel.DataAnnotations;

namespace PerformanceReviewBot.Models;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Display(Name = "Hire Date")]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }

    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";

    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
}
