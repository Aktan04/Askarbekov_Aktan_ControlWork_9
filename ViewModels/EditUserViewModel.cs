using System.ComponentModel.DataAnnotations;

namespace Cw9.ViewModels;

public class EditUserViewModel
{
    public int Id { get; set; }
    [Required]
    public string PersonalAccount { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string UserName { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; }
    
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string? ConfirmPassword { get; set; }
    public bool IsNeededNewPersonalAccount { get; set; }
}