using System;

namespace CarePulse.API.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Actor { get; set; } = string.Empty; // admin username or id
    public string Action { get; set; } = string.Empty; // e.g., "DeleteUser"
    public int? TargetUserId { get; set; }
    public string Details { get; set; } = string.Empty;
}
