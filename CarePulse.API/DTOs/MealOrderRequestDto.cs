using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class MealOrderRequestDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public int MenuItemId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}
