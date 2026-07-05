using System.ComponentModel.DataAnnotations;

namespace CleanIdentity.Web.Models.Account;

public sealed class LoginViewModel
{
    [EmailAddress, Required]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password), Required]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Zapamiętaj mnie")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
