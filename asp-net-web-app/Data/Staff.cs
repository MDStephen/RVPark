using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public class Staff : Users
    {
        public bool isCurrentEmployee { get; set; }
        public bool isAdmin { get; set; }
    }
}
