using asp_net_web_app.Data;
using asp_net_web_app.Services;
using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Services;

public interface IEmailService
{
    Task SendReservationConfirmationAsync(
        string toEmail,
        string customerName,
        Reservations reservation,
        DbSite site);
}

/// <summary>
/// Logs confirmation emails to the console. Replace with SMTP/SendGrid once configured.
/// </summary>
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendReservationConfirmationAsync(
        string toEmail,
        string customerName,
        Reservations reservation,
        DbSite site)
    {
        _logger.LogInformation(
            """
            === RESERVATION CONFIRMATION EMAIL ===
            To: {Email}
            Customer: {Name}
            Reservation #{Id}
            Site: {SiteNumber} ({Category})
            Check-in: {StartDate:MMM d, yyyy}
            Check-out: {EndDate:MMM d, yyyy}
            Total: {TotalCost:C}
            Status: {Status}
            ======================================
            """,
            toEmail, customerName, reservation.Id, site.SiteNumber, site.Category,
            reservation.StartDate, reservation.EndDate, reservation.TotalCost, reservation.Status);

        return Task.CompletedTask;
    }
}
