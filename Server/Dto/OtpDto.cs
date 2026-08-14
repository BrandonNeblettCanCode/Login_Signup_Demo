using System.ComponentModel.DataAnnotations;

namespace Server.Dto
{
    public class OtpDto
    {
        [Required(ErrorMessage = "Please provide a valid OTP")]
        [RegularExpression("^\\d{6}$", ErrorMessage = "Pleas provide a valid OTP")]
        public string? Otp {get; set;} = "000000";

        [EmailAddress(ErrorMessage = "Please a valid 'Email Address'")]
        public string Email {get; set;} = string.Empty;
    }
}