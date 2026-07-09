namespace CarePulse.API.Models;

public class MealOrder
{
    public int MealOrderId { get; set; }
    public int PatientId { get; set; }
    public int MenuItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Patient { get; set; }
    public MenuItem? MenuItem { get; set; }
}
