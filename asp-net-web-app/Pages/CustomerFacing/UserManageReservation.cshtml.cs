using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;
using asp_net_web_app.Services;

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
            public int UserId { get; set; }
            public string CustomerName { get; set; } = "";
            public int SiteId { get; set; }
            public string SiteNumber { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; } = "";
            public decimal TotalCost { get; set; }
        }

        public List<ReservationDisplay> UserReservations { get; set; } = new();
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

        public void OnGetUserCreateReservation(int? id)
        {
            //How to get the current user? 
            //int currentUserId = int.Parse(User.FindFirst("UserId")!.Value);
            AllUsers = _db.Users.ToList();  //.Where(u => u.userId == currentUserId)
            AllSites = _db.Sites.ToList();

            if (id.HasValue)
            {
                CurrentAction = "Edit";
                var res = _db.Reservations.Find(id.Value);
                if (res != null)
                {
                    ActiveId = res.Id;
                    ActiveUserId = res.UserId;
                    ActiveSiteId = res.SiteId;
                    ActiveStartDate = res.StartDate;
                    ActiveEndDate = res.EndDate;
                    ActiveStatus = "Upcoming";
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
            //How to I get the current user? 
            //int currentUserId = int.Parse(User.FindFirst("UserId")!.Value);
            var reservations = _db.Reservations.ToList();  //.Where(r => r.UserId == currentUserId)
            var users = _db.Users.ToList();   //.Where(u => reservations.Select(r => r.UserId).Contains(u.userId))
            var sites = _db.Sites.ToList(); //.Where(s => reservations.Select(r => r.SiteId).Contains(s.Id))

            var today = DateTime.Today;

            foreach (var r in reservations)
            {
                if (DateTime.Today >= r.StartDate && DateTime.Today <= r.EndDate)
                    r.Status = "In Progress";
                else if (DateTime.Today > r.EndDate)
                    r.Status = "Completed";

                _db.SaveChanges();
            }

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

            UserReservations = rows;
        }

        public IActionResult OnPostUserCreateReservation(int userId, int siteId, DateTime startDate, DateTime endDate, string status, decimal totalCost)
        {
            if (AvailablityCheck(startDate, endDate, siteId))
            {
                // Reload dropdowns
                AllUsers = _db.Users.ToList();
                AllSites = _db.Sites.ToList();

                // Restore form mode
                CurrentAction = "Create";

                // Restore user input
                ActiveUserId = userId;
                ActiveSiteId = siteId;
                ActiveStartDate = startDate;
                ActiveEndDate = endDate;
                ActiveStatus = "Upcoming";
                ActiveCost = CalculateCost(siteId, startDate, endDate);

                ModelState.AddModelError("", "This site is already reserved for the selected dates.");
                return Page();
            }
            totalCost = CalculateCost(siteId, startDate, endDate);   
            var newRes = new Reservations
            {
                UserId = userId,
                SiteId = siteId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Upcoming",     // status (what should it be??)
                TotalCost = totalCost
            };
            _db.Reservations.Add(newRes);
            _db.SaveChanges();
            return RedirectToPage("CompleteReservation", new { id = newRes.Id});  
        }

        public IActionResult OnPostUpdateReservation(int reservationId, int userId, int siteId, DateTime startDate, DateTime endDate, string status, decimal totalCost)
        {
            var res = _db.Reservations.Find(reservationId);

            if (AvailablityCheck(startDate, endDate, siteId, reservationId))
            {
                AllUsers = _db.Users.ToList();
                AllSites = _db.Sites.ToList();

                CurrentAction = "Edit";

                ActiveId = reservationId;
                ActiveUserId = userId;
                ActiveSiteId = siteId;
                ActiveStartDate = startDate;
                ActiveEndDate = endDate;
                ActiveStatus = "Upcoming";
                ActiveCost = totalCost;

                ModelState.AddModelError("", "This site is already reserved for the selected dates.");
                return Page();
            }
            totalCost = CalculateCost(siteId, startDate, endDate); 
            if (res != null)
            {
                decimal oldCost = res.TotalCost;

                res.UserId = userId;
                res.SiteId = siteId;
                res.StartDate = startDate;
                res.EndDate = endDate;
                res.Status = "Upcoming";
                res.TotalCost = totalCost;
                _db.SaveChanges();

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
            var res = _db.Reservations.Find(id);
            if (res != null)
            {
                res.Status = "Cancelled";
                _db.SaveChanges();
            }
            return RedirectToPage();
        }
        
        public bool AvailablityCheck(DateTime startDate, DateTime endDate, int siteID, int? ignoreId = null)
        {
            DateTime start = startDate.Date;
            DateTime end = endDate.Date;

            return _db.Reservations
                .Where(r => r.SiteId == siteID)
                .Where(r => ignoreId == null || r.Id != ignoreId)
                .Where(r => start <= r.EndDate && end >= r.StartDate)
                .Any();
        }

        private decimal CalculateCost(int siteId, DateTime start, DateTime end)
        {
            int nights = (end.Date - start.Date).Days;
            if (nights < 1) nights = 1;

            decimal nightlyRate =
                PricingStore.BaseRate *
                PricingStore.SeasonMult *
                PricingStore.LargeSiteMult *
                PricingStore.UtilMult;

            return nightlyRate * nights;
        }

    }
}