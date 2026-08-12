using System.ComponentModel.DataAnnotations;

namespace Server.Dto
{
    public class SignUpDto
    {
        [Required(ErrorMessage = "Invalid 'Username'")]
        [RegularExpression("^[a-zA-Z0-9_-]{8,20}$", ErrorMessage = "'Username' is invalid or too short")]
        public string Username {get; set;} = string.Empty;

        [Required(ErrorMessage = "Invalid 'Password'")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,20}$", ErrorMessage = "'Password' is invalid or too weak")]
        public string Password {get; set;} = string.Empty;

        [EmailAddress(ErrorMessage = "Please provide an valid 'Email Address'")]
        public string Email {get; set;} = string.Empty;
    }
}