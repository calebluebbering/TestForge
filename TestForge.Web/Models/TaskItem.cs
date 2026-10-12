
using System.ComponentModel.DataAnnotations;

namespace TestForge.Web.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
