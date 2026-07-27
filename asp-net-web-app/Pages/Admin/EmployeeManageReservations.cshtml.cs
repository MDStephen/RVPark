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
            public string CustomerEmail { get; set; } = "";
            public string CustomerPhone { get; set; } = "";
            public bool CustomerFound { get; set; }
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

        public class CustomerMatch
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Phone { get; set; } = "";
        }

        public class SiteOption
        {
            public int Id { get; set; }
            public string SiteNumber { get; set; } = "";
        }

        public List<ReservationListItem> AllReservations { get; set; } = new();
        public ReservationDetail? Selected { get; set; }
        public string SearchTerm { get; set; } = "";
        public string StatusMessage { get; set; } = "";
        public bool InvalidId { get; set; }

        // Walk-in reservation workflow state
        public string Mode { get; set; } = "list";
        public string CustomerQuery { get; set; } = "";
        public List<CustomerMatch> CustomerMatches { get; set; } = new();
        public CustomerMatch? SelectedWalkInCustomer { get; set; }
        public List<SiteOption> SiteOptions { get; set; } = new();

        public void OnGet(int? id, string? search, string? status, string? mode, string? custQ, int? custId)
        {
            SearchTerm = search ?? "";
            StatusMessage = status ?? "";
            Mode = mode == "walkin" ? "walkin" : "list";
            LoadList(SearchTerm);

            if (Mode == "walkin")
            {
                CustomerQuery = custQ ?? "";
                SiteOptions = _context.Sites
                    .OrderBy(s => s.SiteNumber)
                    .Select(s => new SiteOption { Id = s.Id, SiteNumber = s.SiteNumber })
                    .ToList();

                if (custId.HasValue)
                {
                    var u = _context.Users.Find(custId.Value);
                    if (u != null)
                    {
                        SelectedWalkInCustomer = new CustomerMatch
                        {
                            Id = u.userId,
                            FullName = $"{u.firstName} {u.lastName}",
                            Email = u.emailAddress,
                            Phone = u.phoneNumber
                        };
                    }
                }
                else if (!string.IsNullOrWhiteSpace(CustomerQuery))
                {
                    var q = CustomerQuery.Trim();
                    CustomerMatches = _context.Users
                        .ToList()
                        .Where(u =>
                            $"{u.firstName} {u.lastName}".Contains(q, StringComparison.OrdinalIgnoreCase) ||
                            (u.emailAddress != null && u.emailAddress.Contains(q, StringComparison.OrdinalIgnoreCase)))
                        .Select(u => new CustomerMatch
                        {
                            Id = u.userId,
                            FullName = $"{u.firstName} {u.lastName}",
                            Email = u.emailAddress,
                            Phone = u.phoneNumber
                        })
                        .ToList();
                }
            }
            else if (id.HasValue)
            {
                Selected = BuildDetail(id.Value);
                InvalidId = Selected == null;
            }
        }

        public IActionResult OnPostCreateCustomer(string? custQ, string? firstName, string? lastName, string? email, string? phone)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(firstName))
            {
                errors.Add("First name is required.");
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                errors.Add("Last name is required.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add("Email is required.");
            }

            if (errors.Count > 0)
            {
                return RedirectToPage(new { mode = "walkin", custQ, status = "Could not create customer: " + string.Join(" ", errors) });
            }

            var customer = new Customer
            {
                firstName = firstName!.Trim(),
                lastName = lastName!.Trim(),
                emailAddress = email!.Trim(),
                phoneNumber = phone ?? "",
                address = ""
            };
            _context.Users.Add(customer);
            _context.SaveChanges();

            return RedirectToPage(new { mode = "walkin", custId = customer.userId });
        }

        public IActionResult OnPostCreateReservation(int custId, int siteId, DateTime startDate, DateTime endDate, int adults, int children, int pets, string? notes, decimal totalCost)
        {
            var customer = _context.Users.Find(custId);
            if (customer == null)
            {
                return RedirectToPage(new { mode = "walkin", status = "Could not create reservation: customer not found." });
            }

            var errors = new List<string>();
            if (siteId <= 0 || _context.Sites.Find(siteId) == null)
            {
                errors.Add("Please select a campsite.");
            }
            if (endDate <= startDate)
            {
                errors.Add("End date must be after start date.");
            }
            if (adults < 1)
            {
                errors.Add("At least 1 adult is required.");
            }
            if (pets < 0 || pets > 4)
            {
                errors.Add("Pets must be between 0 and 4.");
            }
            if (notes != null && notes.Length > 500)
            {
                errors.Add("Notes can't exceed 500 characters.");
            }

            if (errors.Count > 0)
            {
                return RedirectToPage(new { mode = "walkin", custId, status = "Could not create reservation: " + string.Join(" ", errors) });
            }

            var reservation = new Reservations
            {
                UserId = custId,
                SiteId = siteId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Upcoming",
                TotalCost = totalCost,
                Adults = adults,
                Children = children,
                Pets = pets,
                Notes = notes ?? ""
            };
            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            var customerName = $"{customer.firstName} {customer.lastName}";
            return RedirectToPage(new { id = reservation.Id, status = $"Walk-in reservation #{reservation.Id} created for {customerName}" });
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
                CustomerEmail = user?.emailAddress ?? "",
                CustomerPhone = user?.phoneNumber ?? "",
                CustomerFound = user != null,
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
