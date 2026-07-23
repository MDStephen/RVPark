using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using asp_net_web_app.Data;


namespace ReservationService.Pages.Reservations
{
    public class EditReservationModel : PageModel
    {
        private readonly DatabaseWrapper _db;

        public EditReservationModel(DatabaseWrapper db)
        {
            _db = db;
        }

        [BindProperty]
        public Reservation Reservation { get; set; }

        public List<Campsite> AvailableSites { get; set; } = new();

        public decimal OriginalPrice { get; set; }
        public decimal UpdatedPrice { get; set; }
        public decimal PriceDifference => UpdatedPrice - OriginalPrice;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reservation = await _db.Reservations
                .Include(r => r.Campsite)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (Reservation == null)
                return NotFound();

            OriginalPrice = CalculatePrice(Reservation);
            return Page();
        }

        public async Task<IActionResult> OnPostCheckAvailabilityAsync()
        {
            OriginalPrice = CalculatePrice(Reservation);

            var query = _db.Campsites.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Reservation.SiteType))
                query = query.Where(c => c.SiteType == Reservation.SiteType);

            if (Reservation.VehicleLength.HasValue)
                query = query.Where(c => c.MaxLength >= Reservation.VehicleLength.Value);

            query = query.Where(c => !_db.Reservations.Any(r =>
                r.CampsiteId == c.CampsiteId &&
                r.CheckIn < Reservation.CheckOut && Reservation.CheckIn < r.CheckOut &&
                r.ReservationId != Reservation.ReservationId));

            AvailableSites = await query.ToListAsync();

            UpdatedPrice = CalculatePrice(Reservation);
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(int campsiteId)
        {
            var existing = await _db.Reservations.FindAsync(Reservation.ReservationId);
            if (existing == null)
                return NotFound();

            existing.CheckIn = Reservation.CheckIn;
            existing.CheckOut = Reservation.CheckOut;
            existing.SiteType = Reservation.SiteType;
            existing.VehicleLength = Reservation.VehicleLength;
            existing.CampsiteId = campsiteId;

            await _db.SaveChangesAsync();

            return RedirectToPage("ReservationConfirmation", new { id = existing.ReservationId });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var reservation = await _db.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            _db.Reservations.Remove(reservation);
            await _db.SaveChangesAsync();

            return RedirectToPage("ReservationCancelled");
        }

        private decimal CalculatePrice(Reservation r)
        {
            var nights = (r.CheckOut - r.CheckIn).Days;
            decimal baseRate = r.SiteType switch
            {
                "Tent" => 25m,
                "RV" => 40m,
                "Trailer" => 35m,
                "Group Site" => 60m,
                _ => 30m
            };

            return nights * baseRate;
        }
    }
}