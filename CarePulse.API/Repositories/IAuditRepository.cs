using CarePulse.API.Models;

namespace CarePulse.API.Repositories;

public interface IAuditRepository
{
    Task AddAsync(AuditLog log);
    Task SaveChangesAsync();
}
