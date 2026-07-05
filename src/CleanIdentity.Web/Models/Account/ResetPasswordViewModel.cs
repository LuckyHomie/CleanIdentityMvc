using System.ComponentModel.DataAnnotations;

namespace CleanIdentity.Web.Models.Account;

public sealed class ResetPasswordViewModel
{
    [EmailAddress, Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [DataType(DataType.Password), Required, MinLength(12)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Powtórz nowe hasło")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
