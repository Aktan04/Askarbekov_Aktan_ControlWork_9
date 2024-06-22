using System.ComponentModel.DataAnnotations;

namespace Cw9.Models;

public class ServiceUser
{
    public int Id { get; set; }

    [Required]
    public int ServiceProviderId { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }

    [Required]
    public string Identifier { get; set; } 

    public decimal Balance { get; set; }

}