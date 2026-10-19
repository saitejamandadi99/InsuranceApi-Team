using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter the Full Name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "fullname should be between 3 to 50 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter the Email Address")]
        [EmailAddress(ErrorMessage = "Email address is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter the password")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@#$!]).{8,20}$",
        ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a number, and one of these special characters: @, #, $, !")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Mobile Number is requried")]
        [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "phone number must start with numbers between 6-9 must contain exactly 10 digits")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public Role Role { get; set; }

        public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active; //default 

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public virtual CustomerProfile? CustomerProfile { get; set; }
        public virtual ICollection<ClaimStatusHistory>? ClaimStatusHistories { get; set; }

    }
}
