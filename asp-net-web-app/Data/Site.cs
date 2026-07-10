using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public abstract class Site
    {
        [Key]
        public int siteId { get; set; }

        public double length { get; set; }
        public double width { get; set; }
        public bool isAvailable { get; set; }
        public string location { get; set; }
    }
}
