using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public class Users
    {
        [Key]
        public int userId { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
    }
}



