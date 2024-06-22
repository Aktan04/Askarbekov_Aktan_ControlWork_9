namespace Cw9.Models;

public class Transaction
{
    public int Id { get; set; }
    
    public DateTime Date { get; set; }
        
    public decimal Amount { get; set; }
        
    public string Description { get; set; }
        
    public int? FromUserId { get; set; }
    public User? FromUser { get; set; }
    public int ToUserId { get; set; }
    public User? ToUser { get; set; }

}