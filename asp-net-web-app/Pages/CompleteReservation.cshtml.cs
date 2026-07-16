using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages
{
    public class CompleteReservationModel : PageModel
    {
        private readonly DatabaseWrapper _context;

        public CompleteReservationModel(DatabaseWrapper context)
        {
            _context = context;
        }

        public class ReservationSummary
        {
            public string SiteNumber { get; set; } = "Unknown";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int Nights { get; set; }
            public decimal TotalCost { get; set; }
        }

        [BindProperty]
        public int ReservationId { get; set; }

        [BindProperty]
        [Range(1, 20, ErrorMessage = "At least 1 adult is required.")]
        public int Adults { get; set; } = 1;

        [BindProperty]
        [Range(0, 20, ErrorMessage = "Number of children can't be negative.")]
        public int Children { get; set; } = 0;

        [BindProperty]
        [Range(0, 4, ErrorMessage = "Number of pets can't be negative.")]
        public int Pets { get; set; } = 0;

        [BindProperty]
        [StringLength(500, ErrorMessage = "Special requests can't exceed 500 characters.")]
        public string? Notes { get; set; } = "";

        public ReservationSummary? Summary { get; set; }
        public bool ReservationNotFound { get; set; }
        public bool SaveSuccess { get; set; }
        public List<string> ValidationErrors { get; set; } = new();

        public void OnGet(int? id)
        {
            if (!id.HasValue)
            {
                ReservationNotFound = true;
                return;
            }

            var reservation = _context.Reservations.Find(id.Value);
            if (reservation == null)
            {
                ReservationNotFound = true;
                return;
            }

            ReservationId = reservation.Id;
            Adults = reservation.Adults > 0 ? reservation.Adults : 1;
            Children = reservation.Children;
            Pets = reservation.Pets;
            Notes = reservation.Notes;

            Summary = BuildSummary(reservation);
        }

        public IActionResult OnPost()
        {
            var reservation = _context.Reservations.Find(ReservationId);
            if (reservation == null)
            {
                ReservationNotFound = true;
                return Page();
            }

            Summary = BuildSummary(reservation);

            if (Adults < 1)
            {
                ValidationErrors.Add("At least 1 adult is required.");
            }
            if (Children < 0)
            {
                ValidationErrors.Add("Number of children can't be negative.");
            }
            if (Pets < 0)
            {
                ValidationErrors.Add("Number of pets can't be negative.");
            }
            if (Notes != null && Notes.Length > 500)
            {
                ValidationErrors.Add("Special requests can't exceed 500 characters.");
            }

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState.Values)
                {
                    foreach (var error in entry.Errors)
                    {
                        if (!ValidationErrors.Contains(error.ErrorMessage))
                        {
                            ValidationErrors.Add(error.ErrorMessage);
                        }
                    }
                }
            }

            if (ValidationErrors.Count > 0)
            {
                return Page();
            }

            reservation.Adults = Adults;
            reservation.Children = Children;
            reservation.Pets = Pets;
            reservation.Notes = Notes ?? "";
            _context.SaveChanges();

            SaveSuccess = true;
            return Page();
        }

        private ReservationSummary BuildSummary(Reservations reservation)
        {
            var site = _context.Sites.Find(reservation.SiteId);
            var nights = (reservation.EndDate - reservation.StartDate).Days;

            return new ReservationSummary
            {
                SiteNumber = site?.SiteNumber ?? "Unknown",
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                Nights = nights > 0 ? nights : 0,
                TotalCost = reservation.TotalCost
            };
        }
    }
}
