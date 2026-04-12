using System.ComponentModel.DataAnnotations;

namespace Shopfinity.Application.Features.Reviews.DTOs;

public class CreateReviewDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
