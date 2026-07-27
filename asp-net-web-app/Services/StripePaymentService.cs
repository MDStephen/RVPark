using asp_net_web_app.Data;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace asp_net_web_app.Services;

public interface IPaymentService
{
    Task<string> CreateCheckoutSessionAsync(int reservationId, string successUrl, string cancelUrl);
    Task<bool> CompletePaymentAsync(string stripeSessionId);
}

public class StripePaymentService : IPaymentService
{
    private readonly DatabaseWrapper _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        DatabaseWrapper db,
        IEmailService email,
        IConfiguration config,
        ILogger<StripePaymentService> logger)
    {
        _db = db;
        _email = email;
        _config = config;
        _logger = logger;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        int reservationId, string successUrl, string cancelUrl)
    {
        var reservation = await _db.Reservations.FindAsync(reservationId)
            ?? throw new InvalidOperationException("Reservation not found.");

        var site = await _db.Sites.FindAsync(reservation.SiteId);
        var siteLabel = site?.SiteNumber ?? "Site";

        var amountCents = (long)(reservation.TotalCost * 100);
        if (amountCents < 50)
            amountCents = 50;

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = reservationId.ToString(),
            Metadata = new Dictionary<string, string>
            {
                ["reservationId"] = reservationId.ToString()
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = amountCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"RV Park Reservation  {siteLabel}",
                            Description = $"{reservation.StartDate:MMM d} – {reservation.EndDate:MMM d, yyyy}"
                        }
                    }
                }
            ]
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task<bool> CompletePaymentAsync(string stripeSessionId)
    {
        var sessionService = new SessionService();
        var session = await sessionService.GetAsync(stripeSessionId);

        if (session.PaymentStatus != "paid")
            return false;

        if (!int.TryParse(session.Metadata.GetValueOrDefault("reservationId"), out var reservationId))
        {
            if (!int.TryParse(session.ClientReferenceId, out reservationId))
                return false;
        }

        var reservation = await _db.Reservations.FindAsync(reservationId);
        if (reservation == null)
            return false;

        var existing = await _db.Payments
            .AnyAsync(p => p.stripeId == session.PaymentIntentId || p.stripeId == session.Id);
        if (existing)
            return true;

        var payment = new Payment
        {
            amount = reservation.TotalCost,
            paidAt = DateTime.UtcNow,
            stripeId = session.PaymentIntentId ?? session.Id,
            paymentStatus = "paid",
            ReservationId = reservationId
        };

        _db.Payments.Add(payment);
        reservation.Status = "Confirmed";
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(reservation.UserId);
        var site = await _db.Sites.FindAsync(reservation.SiteId);

        if (user != null && site != null && !string.IsNullOrWhiteSpace(user.emailAddress))
        {
            await _email.SendReservationConfirmationAsync(
                user.emailAddress,
                $"{user.firstName} {user.lastName}",
                reservation,
                site);
        }

        _logger.LogInformation("Payment recorded for reservation #{ReservationId}", reservationId);
        return true;
    }
}
