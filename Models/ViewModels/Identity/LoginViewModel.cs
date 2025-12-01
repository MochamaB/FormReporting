using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Identity
{
    /// <summary>
    /// ViewModel for user login
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Username or Email address
        /// </summary>
        [Required(ErrorMessage = "Username or Email is required")]
        [Display(Name = "Username or Email")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// User password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Remember me option
        /// </summary>
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;

        /// <summary>
        /// URL to redirect to after successful login
        /// Used for deep linking and preserving user's intended destination
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}
