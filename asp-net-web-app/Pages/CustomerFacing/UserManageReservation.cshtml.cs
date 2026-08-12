using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages
{
    public class UserManageReservationsModel : PageModel
    {
        private readonly DatabaseWrapper _db;

        public UserManageReservationsModel(DatabaseWrapper db)
        {
            _db = db;
        }

        public class ReservationDisplay
        {
            public int Id { get; set; }
            public int SiteId { get; set; }
            public string SiteNumber { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; } = "";
            public decimal TotalCost { get; set; }
        }

        public List<ReservationDisplay> UserReservations { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string StatusMessage { get; set; } = "";

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int id))
            {
                return id;
            }
            return 0;
        }

        public void OnGet(string? search, string? status)
        {
            SearchTerm = search ?? "";
            StatusMessage = status ?? "";
            LoadUserReservations(SearchTerm);
        }

        public IActionResult OnPostCancel(int id, string? search)
        {
            int currentUserId = GetCurrentUserId();
            var reservation = _db.Reservations.Find(id);

            // Ownership check: a user may only cancel their own reservation.
            if (reservation == null || reservation.UserId != currentUserId)
            {
                return RedirectToPage(new { search, status = "Could not cancel: reservation not found." });
            }

            if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
            {
                return RedirectToPage(new { search, status = $"Reservation #{id} can no longer be cancelled." });
            }

            reservation.Status = "Cancelled";
            _db.SaveChanges();

            return RedirectToPage(new { search, status = $"Reservation #{id} has been cancelled." });
        }

        private void LoadUserReservations(string search)
        {
            int currentUserId = GetCurrentUserId();

            // Strictly filter by the logged-in user's ID
            var reservations = _db.Reservations
                .Where(r => r.UserId == currentUserId)
                .ToList();

            var sites = _db.Sites.ToList();

            // Update statuses dynamically based on dates
            foreach (var r in reservations)
            {
                if (r.Status != "Cancelled")
                {
                    if (DateTime.Today >= r.StartDate && DateTime.Today <= r.EndDate)
                        r.Status = "In Progress";
                    else if (DateTime.Today > r.EndDate)
                        r.Status = "Completed";
                }
            }

            _db.SaveChanges();

            var rows = reservations.Select(r => new ReservationDisplay
            {
                Id = r.Id,
                SiteId = r.SiteId,
                SiteNumber = sites.FirstOrDefault(s => s.Id == r.SiteId)?.SiteNumber ?? "Unknown",
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status,
                TotalCost = r.TotalCost
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                rows = rows.Where(r =>
                    r.Id.ToString() == search ||
                    r.SiteNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            UserReservations = rows;
        }
    }
}
