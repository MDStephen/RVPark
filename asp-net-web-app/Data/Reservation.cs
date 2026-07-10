using System;
using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public class Reservation
    {
        [Key]
        public int reservationId { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string status { get; set; }
        public decimal totalCost { get; set; }
        public bool isEligible { get; set; }
    }
}
