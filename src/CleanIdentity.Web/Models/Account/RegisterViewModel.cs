using System.ComponentModel.DataAnnotations;

namespace CleanIdentity.Web.Models.Account;

public sealed class RegisterViewModel
{
    [EmailAddress, Required]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Imię")]
    public string? FirstName { get; set; }

    [Display(Name = "Nazwisko")]
    public string? LastName { get; set; }

    [DataType(DataType.Password), Required, MinLength(12)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Powtórz hasło")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
