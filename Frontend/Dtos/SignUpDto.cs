using System.ComponentModel.DataAnnotations;

namespace Frontend.Dtos
{
    public class SignUpDto
    {
        [Required(ErrorMessage = "Please provide a valid 'Username'")]
        [RegularExpression("", ErrorMessage = "Invalid 'Username'")]
        public string Username {get; set;} = string.Empty;

        [Required(ErrorMessage = "Please provide a valid 'Email Address'")]
        [EmailAddress(ErrorMessage = "Invalid 'Email Address'")]
        public string Email {get; set;}  = string.Empty;

        [Required(ErrorMessage = "Please provide a valid 'Password'")]
        [RegularExpression("", ErrorMessage = "Invalid 'Password'")]
        public string Password {get; set;} = string.Empty;

        [Required(ErrorMessage = "Please re-enter your 'Password'")]
        [Compare("Password", ErrorMessage = "Passwords do not match, try again")]
        public string ConfirmPassword {get; set;} = string.Empty;
    }
}