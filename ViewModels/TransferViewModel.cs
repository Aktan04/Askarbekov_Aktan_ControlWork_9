using System.ComponentModel.DataAnnotations;

namespace Cw9.ViewModels;

public class TransferViewModel
{
    [Required(ErrorMessage = "Пожалуйста, введите номер счета получателя")]
    public int RecipientAccountNumber { get; set; }

    [Required(ErrorMessage = "Пожалуйста, введите сумму перевода")]
    [Range(1, int.MaxValue, ErrorMessage = "Сумма перевода должна быть больше нуля")]
    public int Amount { get; set; }
}