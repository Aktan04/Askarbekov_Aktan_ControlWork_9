using System.ComponentModel.DataAnnotations;

namespace Cw9.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Не указан Email или логин")]
    public string EmailOrLogin { get; set; }
    [Required(ErrorMessage = "Не указан пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}