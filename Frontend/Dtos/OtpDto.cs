using System.ComponentModel.DataAnnotations;

namespace Frontend.Dtos
{
    public class OtpDto
    {
        [EmailAddress(ErrorMessage = "Please provide a valid 'Email Address'")]
        public string? Email {get; set;} = string.Empty;

        [RegularExpression("^\\d{6}$", ErrorMessage = "Please provide a valids 'OTP'")]
        public string? Otp {get; set;} = string.Empty;
    }
}