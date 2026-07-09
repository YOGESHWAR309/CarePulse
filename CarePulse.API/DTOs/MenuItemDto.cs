using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class MenuItemDto
{
    [Required]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required]
    public string FoodCategory { get; set; } = "General";

    public bool IsAvailable { get; set; } = true;
}
