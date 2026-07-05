using System.ComponentModel.DataAnnotations;

namespace CleanIdentity.Web.Models.Account;

public sealed class ChangePasswordViewModel
{
    [DataType(DataType.Password), Required, Display(Name = "Aktualne hasło")]
    public string CurrentPassword { get; set; } = string.Empty;

    [DataType(DataType.Password), Required, MinLength(12), Display(Name = "Nowe hasło")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Powtórz nowe hasło")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool Expired { get; set; }
}
