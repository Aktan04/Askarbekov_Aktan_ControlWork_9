using System.ComponentModel.DataAnnotations;

namespace Cw9.Models;

public class ServiceProvider
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public ICollection<ServiceUser> ServiceUsers { get; set; }
}