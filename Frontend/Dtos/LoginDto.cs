using System.ComponentModel.DataAnnotations;

namespace Frontend.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Please provide a valid 'Username'")]
        [RegularExpression("", ErrorMessage = "Invalid 'Username'")]
        public string Username = string.Empty;

        [Required(ErrorMessage = "Please provide a valid 'Password'")]
        [RegularExpression("", ErrorMessage = "Invalid 'Password'")]
        public string Password = string.Empty;
    }
}