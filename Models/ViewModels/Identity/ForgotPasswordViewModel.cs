using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Identity
{
    /// <summary>
    /// ViewModel for forgot password request
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}
