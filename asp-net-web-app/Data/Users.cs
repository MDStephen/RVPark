using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public class Users
    {
        [Key]
        public int userId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string firstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string lastName { get; set; }

        public string? middleInitial { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string emailAddress { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string phoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string address { get; set; }

        public string? aptSuite { get; set; }
        
    }
}
