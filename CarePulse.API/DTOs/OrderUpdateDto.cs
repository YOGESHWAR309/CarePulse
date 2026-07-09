using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class OrderUpdateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "OrderId must be a positive value.")]
    public int OrderId { get; set; }

    [Required]
    [MinLength(3, ErrorMessage = "Status must be at least 3 characters long.")]
    public string Status { get; set; } = string.Empty;
}
