using System.ComponentModel.DataAnnotations;

namespace Frontend.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Please provide a valid 'Username'")]
        [RegularExpression("^[a-zA-Z0-9_]{3,20}$", ErrorMessage = "Invalid 'Usernames'")]
        public string Username  {get; set;} = string.Empty;

        [Required(ErrorMessage = "Please provide a valid 'Password'")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Invalid 'Password'")]
        public string Password {get; set;} = string.Empty;
    }
}