using System.ComponentModel.DataAnnotations;

namespace Server.Model
{
    public class Users
    {
        public Guid Id {get; set;} = Guid.CreateVersion7();
        public string Username {get; set;} = string.Empty;
        public string Password {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        public string? Otp {get; set;} = string.Empty;
    }
}