using System.Collections.Concurrent;
using CarePulse.API.Repositories;
using Microsoft.Extensions.Hosting;

namespace CarePulse.API.Services;

public class AppointmentReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AppointmentReminderBackgroundService> _logger;
    private readonly ConcurrentDictionary<int, DateTime> _lastReminderSent = new();

    public AppointmentReminderBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AppointmentReminderBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Appointment reminder background job failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();
        var careRepository = scope.ServiceProvider.GetRequiredService<ICareRepository>();

        var now = DateTime.UtcNow;
        var appointments = await careRepository.GetAllAppointmentsAsync();

        foreach (var appointment in appointments.Where(x =>
                     !string.Equals(x.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(x.Status, "Completed", StringComparison.OrdinalIgnoreCase) &&
                     x.AppointmentDate > now &&
                     x.AppointmentDate <= now.AddHours(24)))
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            if (_lastReminderSent.TryGetValue(appointment.AppointmentId, out var lastSent) && lastSent > now.AddHours(-12))
                continue;

            try
            {
                await appointmentService.SendAppointmentReminderAsync(appointment.AppointmentId);
                _lastReminderSent[appointment.AppointmentId] = now;
                _logger.LogInformation("Sent reminder for appointment {AppointmentId}", appointment.AppointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to send reminder for appointment {AppointmentId}", appointment.AppointmentId);
            }
        }
    }
}
