using System.ComponentModel.DataAnnotations;

namespace Frontend.Dtos
{
    public class OtpDto
    {
        [EmailAddress(ErrorMessage = "Please provide a valid 'Email Address'")]
        public string? Email {get; set;} = string.Empty;

        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Please provide a valid 'OTP'")]
        public string? Otp {get; set;} = string.Empty;
    }
}