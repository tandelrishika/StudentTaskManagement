using System.ComponentModel.DataAnnotations;

namespace StudentTaskManagement.Models;

public class Assignment
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Subject")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a subject.")]
    public int SubjectId { get; set; }

    public Subject? Subject { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

    [Required]
    [RegularExpression("Pending|In Progress|Completed",
        ErrorMessage = "Choose a valid status.")]
    public string Status { get; set; } = "Pending";

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}