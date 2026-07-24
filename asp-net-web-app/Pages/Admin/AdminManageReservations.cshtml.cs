using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages
{
    public class AdminManageReservationsModel : PageModel
    {
        private readonly DatabaseWrapper _context;

        public AdminManageReservationsModel(DatabaseWrapper context)
        {
            _context = context;
        }

        public class ReservationDisplay
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string CustomerName { get; set; } = "";
            public int SiteId { get; set; }
            public string SiteNumber { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; } = "";
            public decimal TotalCost { get; set; }
        }

        public List<ReservationDisplay> AllReservations { get; set; } = new();
        public List<Users> AllUsers { get; set; } = new();
        public List<DbSite> AllSites { get; set; } = new();

        public string CurrentAction { get; set; } = "List";
        public string SearchTerm { get; set; } = "";
        public decimal? BalanceDiff { get; set; }

        public int ActiveId { get; set; }
        public int ActiveUserId { get; set; }
        public int ActiveSiteId { get; set; }
        public DateTime ActiveStartDate { get; set; } = DateTime.Today;
        public DateTime ActiveEndDate { get; set; } = DateTime.Today.AddDays(1);
        public string ActiveStatus { get; set; } = "Upcoming";
        public decimal ActiveCost { get; set; }

        public void OnGet(string? search)
        {
            CurrentAction = "List";
            SearchTerm = search ?? "";
            LoadDisplayList(SearchTerm);

            if (TempData["BalanceDiff"] != null)
            {
                BalanceDiff = decimal.Parse(TempData["BalanceDiff"]!.ToString()!);
            }
        }

        public void OnGetCreateReservation(int? id)
        {
            AllUsers = _context.Users.ToList();
            AllSites = _context.Sites.ToList();

            if (id.HasValue)
            {
                CurrentAction = "Edit";
                var res = _context.Reservations.Find(id.Value);
                if (res != null)
                {
                    ActiveId = res.Id;
                    ActiveUserId = res.UserId;
                    ActiveSiteId = res.SiteId;
                    ActiveStartDate = res.StartDate;
                    ActiveEndDate = res.EndDate;
                    ActiveStatus = res.Status;
                    ActiveCost = res.TotalCost;
                }
            }
            else
            {
                CurrentAction = "Create";
                ActiveId = 0;
            }
        }

        private void LoadDisplayList(string search)
        {
            var reservations = _context.Reservations.ToList();
            var users = _context.Users.ToList();
            var sites = _context.Sites.ToList();

            var rows = reservations.Select(r => new ReservationDisplay
            {
                Id = r.Id,
                UserId = r.UserId,
                CustomerName = users.FirstOrDefault(u => u.userId == r.UserId) is { } u ? $"{u.firstName} {u.lastName}" : "Unknown",
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
                    r.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            AllReservations = rows;
        }

        public IActionResult OnPostCreateReservation(int userId, int siteId, DateTime startDate, DateTime endDate, string status, decimal totalCost)
        {
            var newRes = new Reservations
            {
                UserId = userId,
                SiteId = siteId,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                TotalCost = totalCost
            };
            _context.Reservations.Add(newRes);
            _context.SaveChanges();
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateReservation(int reservationId, int userId, int siteId, DateTime startDate, DateTime endDate, string status, decimal totalCost)
        {
            var res = _context.Reservations.Find(reservationId);
            if (res != null)
            {
                decimal oldCost = res.TotalCost;

                res.UserId = userId;
                res.SiteId = siteId;
                res.StartDate = startDate;
                res.EndDate = endDate;
                res.Status = status;
                res.TotalCost = totalCost;
                _context.SaveChanges();

                decimal diff = totalCost - oldCost;
                if (diff != 0)
                {
                    TempData["BalanceDiff"] = diff.ToString();
                }
            }
            return RedirectToPage();
        }

        public IActionResult OnPostCancelReservation(int id)
        {
            var res = _context.Reservations.Find(id);
            if (res != null)
            {
                res.Status = "Cancelled";
                _context.SaveChanges();
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteReservation(int id)
        {
            var res = _context.Reservations.Find(id);
            if (res != null)
            {
                _context.Reservations.Remove(res);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }
    }
}