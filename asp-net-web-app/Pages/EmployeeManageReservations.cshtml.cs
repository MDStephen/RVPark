using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages
{
    public class EmployeeManageReservationsModel : PageModel
    {
        private readonly DatabaseWrapper _context;

        public EmployeeManageReservationsModel(DatabaseWrapper context)
        {
            _context = context;
        }

        public class ReservationListItem
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; } = "";
        }

        public class ReservationDetail
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string SiteNumber { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int Nights { get; set; }
            public decimal TotalCost { get; set; }
            public string Status { get; set; } = "";
            public int Adults { get; set; }
            public int Children { get; set; }
            public int Pets { get; set; }
            public string Notes { get; set; } = "";
        }

        public List<ReservationListItem> AllReservations { get; set; } = new();
        public ReservationDetail? Selected { get; set; }
        public string SearchTerm { get; set; } = "";
        public string StatusMessage { get; set; } = "";
        public bool InvalidId { get; set; }

        public void OnGet(int? id, string? search, string? status)
        {
            SearchTerm = search ?? "";
            StatusMessage = status ?? "";
            LoadList(SearchTerm);

            if (id.HasValue)
            {
                Selected = BuildDetail(id.Value);
                InvalidId = Selected == null;
            }
        }

        public IActionResult OnPostSave(int id, DateTime startDate, DateTime endDate, int adults, int children, int pets, string? notes)
        {
            var res = _context.Reservations.Find(id);
            if (res == null)
            {
                return RedirectToPage(new { status = "Reservation not found." });
            }

            var errors = new List<string>();
            if (endDate <= startDate)
            {
                errors.Add("End date must be after start date.");
            }
            if (adults < 1)
            {
                errors.Add("At least 1 adult is required.");
            }

            if (errors.Count > 0)
            {
                return RedirectToPage(new { id, status = "Could not save: " + string.Join(" ", errors) });
            }

            res.StartDate = startDate;
            res.EndDate = endDate;
            res.Adults = adults;
            res.Children = children;
            res.Pets = pets;
            res.Notes = notes ?? "";
            _context.SaveChanges();

            return RedirectToPage(new { id, status = "Changes saved." });
        }

        public IActionResult OnPostCancel(int id)
        {
            var res = _context.Reservations.Find(id);
            if (res != null)
            {
                res.Status = "Cancelled";
                _context.SaveChanges();
            }
            return RedirectToPage(new { id, status = "Reservation cancelled." });
        }

        public IActionResult OnPostReactivate(int id)
        {
            var res = _context.Reservations.Find(id);
            if (res != null)
            {
                res.Status = "Upcoming";
                _context.SaveChanges();
            }
            return RedirectToPage(new { id, status = "Reservation reactivated." });
        }

        private void LoadList(string search)
        {
            var reservations = _context.Reservations.ToList();
            var users = _context.Users.ToList();

            var rows = reservations.Select(r => new ReservationListItem
            {
                Id = r.Id,
                CustomerName = users.FirstOrDefault(u => u.userId == r.UserId) is { } u ? $"{u.firstName} {u.lastName}" : "Unknown",
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status
            }).OrderByDescending(r => r.StartDate).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                rows = rows.Where(r =>
                    r.Id.ToString() == search ||
                    r.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            AllReservations = rows;
        }

        private ReservationDetail? BuildDetail(int id)
        {
            var res = _context.Reservations.Find(id);
            if (res == null)
            {
                return null;
            }

            var user = _context.Users.FirstOrDefault(u => u.userId == res.UserId);
            var site = _context.Sites.FirstOrDefault(s => s.Id == res.SiteId);
            var nights = (res.EndDate - res.StartDate).Days;

            return new ReservationDetail
            {
                Id = res.Id,
                CustomerName = user != null ? $"{user.firstName} {user.lastName}" : "Unknown",
                SiteNumber = site?.SiteNumber ?? "Unknown",
                StartDate = res.StartDate,
                EndDate = res.EndDate,
                Nights = nights > 0 ? nights : 0,
                TotalCost = res.TotalCost,
                Status = res.Status,
                Adults = res.Adults,
                Children = res.Children,
                Pets = res.Pets,
                Notes = res.Notes
            };
        }
    }
}
