using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using asp_net_web_app.Data;
using asp_net_web_app.Repositories;

namespace asp_net_web_app.Pages
{
    public class AdminManageReservationsModel : PageModel
    {
        private readonly DatabaseWrapper _context;
        private readonly IUserRepository _userRepository;

        public AdminManageReservationsModel(DatabaseWrapper context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
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

        public class CustomerOption
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Phone { get; set; } = "";
        }

        public class ReservationDetailsDto
        {
            public int Id { get; set; }
            public string Status { get; set; } = "";
            public string CustomerName { get; set; } = "Unknown";
            public string CustomerEmail { get; set; } = "";
            public string CustomerPhone { get; set; } = "";
            public bool CustomerFound { get; set; }
            public string SiteNumber { get; set; } = "Unknown";
            public string SiteType { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int Nights { get; set; }
            public decimal RatePerNight { get; set; }
            public decimal TotalCost { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal BalanceOwed { get; set; }
            public List<PaymentDetailsDto> Payments { get; set; } = new();
        }

        public class PaymentDetailsDto
        {
            public decimal Amount { get; set; }
            public DateTime PaidAt { get; set; }
            public string Status { get; set; } = "";
            public string Reference { get; set; } = "";
        }

        public List<ReservationDisplay> AllReservations { get; set; } = new();
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

        // Customer search / walk-in creation state
        public string CustomerQuery { get; set; } = "";
        public List<CustomerOption> CustomerMatches { get; set; } = new();
        public CustomerOption? SelectedCustomer { get; set; }
        public CustomerOption? DuplicateEmailCustomer { get; set; }
        public List<string> CustomerErrors { get; set; } = new();
        public string NewCustomerFirstName { get; set; } = "";
        public string NewCustomerLastName { get; set; } = "";
        public string NewCustomerEmail { get; set; } = "";
        public string NewCustomerPhone { get; set; } = "";
        public string NewCustomerAddress { get; set; } = "";

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

        public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.userId == res.UserId);
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.Id == res.SiteId);
            var nights = Math.Max(0, (res.EndDate - res.StartDate).Days);

            var payments = await _context.Payments
                .Where(p => p.ReservationId == res.Id)
                .OrderBy(p => p.paidAt)
                .Select(p => new PaymentDetailsDto
                {
                    Amount = p.amount,
                    PaidAt = p.paidAt,
                    Status = p.paymentStatus,
                    Reference = p.stripeId
                })
                .ToListAsync();
            var amountPaid = payments.Sum(p => p.Amount);

            var dto = new ReservationDetailsDto
            {
                Id = res.Id,
                Status = res.Status,
                CustomerName = user != null ? $"{user.firstName} {user.lastName}" : "Unknown",
                CustomerEmail = user?.emailAddress ?? "",
                CustomerPhone = user?.phoneNumber ?? "",
                CustomerFound = user != null,
                SiteNumber = site?.SiteNumber ?? "Unknown",
                SiteType = site?.Category ?? "",
                StartDate = res.StartDate,
                EndDate = res.EndDate,
                Nights = nights,
                RatePerNight = nights > 0 ? Math.Round(res.TotalCost / nights, 2) : 0,
                TotalCost = res.TotalCost,
                AmountPaid = amountPaid,
                BalanceOwed = res.TotalCost - amountPaid,
                Payments = payments
            };

            return new JsonResult(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task OnGetCreateReservation(int? id, string? custQ, int? custId, bool changeCustomer = false)
        {
            AllSites = _context.Sites.ToList();
            CustomerQuery = custQ ?? "";

            if (id.HasValue && id.Value > 0)
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

            if (custId.HasValue)
            {
                ActiveUserId = custId.Value;
            }
            if (changeCustomer)
            {
                ActiveUserId = 0;
            }

            if (ActiveUserId > 0)
            {
                var u = await _userRepository.GetByIdAsync(ActiveUserId);
                if (u != null)
                {
                    SelectedCustomer = ToCustomerOption(u);
                }
            }

            if (SelectedCustomer == null && !string.IsNullOrWhiteSpace(CustomerQuery))
            {
                var matches = await _userRepository.SearchAsync(CustomerQuery);
                CustomerMatches = matches.Select(ToCustomerOption).ToList();
            }
        }

        public async Task<IActionResult> OnPostCreateCustomer(int? reservationId, string? custQ, string? firstName, string? lastName, string? email, string? phone, string? address)
        {
            firstName = (firstName ?? "").Trim();
            lastName = (lastName ?? "").Trim();
            email = (email ?? "").Trim();
            phone = (phone ?? "").Trim();
            address = (address ?? "").Trim();

            var errors = new List<string>();
            Users? existingByEmail = null;

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
            else if (!new EmailAddressAttribute().IsValid(email))
            {
                errors.Add("Enter a valid email address.");
            }
            else
            {
                existingByEmail = await _userRepository.GetByEmailAsync(email);
                if (existingByEmail != null)
                {
                    errors.Add("A customer with this email already exists.");
                }
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                errors.Add("Phone number is required.");
            }
            else if (Regex.Replace(phone, "[^0-9]", "").Length != 10)
            {
                errors.Add("Phone number must be a valid 10-digit number.");
            }

            // Re-establish page state in case we need to re-render the form.
            CurrentAction = reservationId.HasValue && reservationId.Value > 0 ? "Edit" : "Create";
            ActiveId = reservationId ?? 0;
            CustomerQuery = custQ ?? "";
            AllSites = _context.Sites.ToList();
            if (ActiveId > 0)
            {
                var res = _context.Reservations.Find(ActiveId);
                if (res != null)
                {
                    ActiveSiteId = res.SiteId;
                    ActiveStartDate = res.StartDate;
                    ActiveEndDate = res.EndDate;
                    ActiveStatus = res.Status;
                    ActiveCost = res.TotalCost;
                }
            }

            if (errors.Count > 0)
            {
                CustomerErrors = errors;
                NewCustomerFirstName = firstName;
                NewCustomerLastName = lastName;
                NewCustomerEmail = email;
                NewCustomerPhone = phone;
                NewCustomerAddress = address;
                if (existingByEmail != null)
                {
                    DuplicateEmailCustomer = ToCustomerOption(existingByEmail);
                }
                return Page();
            }

            var newCustomer = new Users
            {
                firstName = firstName,
                lastName = lastName,
                emailAddress = email,
                phoneNumber = phone,
                address = address
            };
            await _userRepository.CreateAsync(newCustomer);

            int? redirectId = reservationId.HasValue && reservationId.Value > 0 ? reservationId : null;
            return RedirectToPage("", "CreateReservation", new { id = redirectId, custId = newCustomer.userId });
        }

        private static CustomerOption ToCustomerOption(Users u) => new CustomerOption
        {
            Id = u.userId,
            FullName = $"{u.firstName} {u.lastName}",
            Email = u.emailAddress,
            Phone = u.phoneNumber
        };

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