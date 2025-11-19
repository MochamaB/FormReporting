using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Identity
{
    /// <summary>
    /// ViewModel for resetting password
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password reset token
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirm new password
        /// </summary>
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
