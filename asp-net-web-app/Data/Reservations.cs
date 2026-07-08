using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public class Reservations
    {
        [Key]
        public int ReservationId { get; set; }

        public string   startDate   { get; set; }
        public string   endDate     { get; set; }
        public string   status      { get; set; }
        public int      totalCost   { get; set; }
        public bool     isEligible  { get; set; }
    }
}

