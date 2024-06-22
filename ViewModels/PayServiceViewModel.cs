using System.ComponentModel.DataAnnotations;

namespace Cw9.ViewModels;

public class PayServiceViewModel
{
    [Required]
    public int ServiceProviderId { get; set; }

    [Required]
    public string Identifier { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше нуля")]
    public decimal Amount { get; set; }
}