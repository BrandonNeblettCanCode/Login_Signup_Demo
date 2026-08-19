using System.ComponentModel.DataAnnotations;

namespace Server.Dto
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Invalid 'Username'")]
        public string Username {get; set;} =  string.Empty;
        
        [Required(ErrorMessage = "Invalid 'Password'")]
        public string Password {get; set;} = string.Empty;
    }
}