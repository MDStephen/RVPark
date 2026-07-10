using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public abstract class Users
    {
        [Key]
        public int userId { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string streetAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public bool isBanned { get; set; }
    }
}
