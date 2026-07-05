using System.ComponentModel.DataAnnotations;

namespace CleanIdentity.Web.Models.Account;

public sealed class ForgotPasswordViewModel
{
    [EmailAddress, Required]
    public string Email { get; set; } = string.Empty;
}
