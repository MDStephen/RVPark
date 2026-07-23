using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservationService.Pages.Reservations
{
    public class CreateReservationModel : PageModel
    {
        private readonly DatabaseWrapper _db;

        public CreateReservationModel(DatabaseWrapper db)
        {
            _db = db;
        }

        [BindProperty]
        public DateTime CheckIn { get; set; }

        [BindProperty]
        public DateTime CheckOut { get; set; }

        [BindProperty]
        public string SiteType { get; set; }

        [BindProperty]
        public int? VehicleLength { get; set; }

        public List<Campsite> MatchingSites { get; set; } = new();

        public List<string> SiteTypes { get; set; } = new()
        {
            "Tent",
            "RV",
            "Trailer",
            "Group Site"
        };

        public async Task OnPostSearchAsync()
        {
            var query = _db.Campsites.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SiteType))
                query = query.Where(c => c.SiteType == SiteType);

            if (VehicleLength.HasValue)
                query = query.Where(c => c.MaxLength >= VehicleLength.Value);

            query = query.Where(c => !_db.Reservations.Any(r =>
                r.CampsiteId == c.CampsiteId &&
                r.CheckIn < CheckOut && CheckIn < r.CheckOut));

            MatchingSites = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostReserveAsync(int campsiteId)
        {
            var reservation = new Reservation
            {
                CampsiteId = campsiteId,
                CheckIn = CheckIn,
                CheckOut = CheckOut,
                VehicleLength = VehicleLength,
                SiteType = SiteType,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            return RedirectToPage("ReservationConfirmation", new { id = reservation.ReservationId });
        }
    }
}