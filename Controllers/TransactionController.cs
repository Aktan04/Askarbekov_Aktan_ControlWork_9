using Cw9.Models;
using Cw9.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cw9.Controllers;

public class TransactionController : Controller
{
    private readonly WalletContext _context;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public TransactionController(UserManager<User> userManager, SignInManager<User> signInManager, WalletContext context)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        var transactionsQuery = _context.Transactions
            .Where(t => t.FromUserId == user.Id || (t.ToUserId == user.Id && t.FromUserId == null));

        if (fromDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date <= toDate.Value);
        }

        var transactions = await transactionsQuery
            .Include(t => t.FromUser)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return View(transactions);
    }
    
    [HttpGet]
    public IActionResult Transfer()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(TransferViewModel model)
    {
        var sender = await _userManager.GetUserAsync(User);
        var recipient = await _context.Users.FirstOrDefaultAsync(u => u.PersonalAccount == model.RecipientAccountNumber);

        if (recipient == null)
        {
            ModelState.AddModelError("RecipientAccountNumber", "Счет получателя не найден");
            return View();
        }


        Transaction transactionSender = new Transaction();
        recipient.Balance += model.Amount;
        Transaction transactionRecipient = new Transaction
        {
            ToUserId = recipient.Id,
            Date = DateTime.UtcNow.AddHours(6),
            Amount = model.Amount,
            Description = "Пополнение счета",
        };
        if (sender != null)
        {
            if (sender.PersonalAccount == recipient.PersonalAccount)
            {
                ModelState.AddModelError("RecipientAccountNumber", "Нельзя перевести деньги со своего счета на свой счет");
                return View();
            }
            if (sender.Balance < model.Amount)
            {
                ModelState.AddModelError("Amount", "Недостаточно средств для перевода");
                return View();
            }
            sender.Balance -= model.Amount;

            transactionSender.FromUserId = sender.Id;
            transactionSender.Date = DateTime.UtcNow.AddHours(6);
            transactionSender.Amount = -model.Amount;
            transactionSender.Description =
                $"Перевод средств на счет {model.RecipientAccountNumber}({recipient.UserName})";
            transactionSender.ToUserId = recipient.Id;
            
            transactionRecipient.Description =
                $"Получение средств от счета {sender.PersonalAccount}({sender.UserName})";
            _context.Transactions.Add(transactionSender);
            _context.Users.Update(sender);

        }

        _context.Transactions.Add(transactionRecipient);
        _context.Users.Update(recipient);
        await _context.SaveChangesAsync();
        ViewBag.Message = "Перевод успешно выполнен";
        return View();
    }
}