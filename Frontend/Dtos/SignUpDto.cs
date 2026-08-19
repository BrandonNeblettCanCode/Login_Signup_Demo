using System.ComponentModel.DataAnnotations;

namespace Frontend.Dtos
{
    public class SignUpDto
    {
        [Required(ErrorMessage = "Please provide a valid 'Username'")]
        [RegularExpression("^[a-zA-Z0-9_]{3,20}$", ErrorMessage = "Invalid 'Username'")]
        public string Username {get; set;} = string.Empty;

        [Required(ErrorMessage = "Please provide a valid 'Email Address'")]
        [EmailAddress(ErrorMessage = "Invalid 'Email Address'")]
        public string Email {get; set;}  = string.Empty;

        [Required(ErrorMessage = "Please provide a valid 'Password'")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Invalid 'Password'")]
        public string Password {get; set;} = string.Empty;

        [Required(ErrorMessage = "Please re-enter your 'Password'")]
        [Compare("Password", ErrorMessage = "Passwords do not match, try again")]
        public string ConfirmPassword {get; set;} = string.Empty;
    }
}