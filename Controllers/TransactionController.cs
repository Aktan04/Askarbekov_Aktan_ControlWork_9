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
        var recipient = await _context.Users.FirstOrDefaultAsync(u => u.PersonalAccount.Equals(model.RecipientAccountNumber));

        if (recipient == null)
        {
            ModelState.AddModelError("RecipientAccountNumber", "Счет получателя не найден");
            return View();
        }
        if (model.Amount < 0)
        {
            ModelState.AddModelError("Amount", "Сумма не может быть отрицательной");
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
    
     [HttpGet]
     [Authorize]
     public async Task<IActionResult> PayService()
     {
         var serviceProviders = await _context.ServiceProviders.ToListAsync();
         ViewBag.ServiceProviders = serviceProviders;
         return View();
     }

     [HttpPost]
     [Authorize]
     [ValidateAntiForgeryToken]
     public async Task<IActionResult> PayService(PayServiceViewModel model)
     {
         if (!ModelState.IsValid)
         {
             var serviceProviders = await _context.ServiceProviders.ToListAsync();
             ViewBag.ServiceProviders = serviceProviders;
             return View(model);
         }

         var user = await _userManager.GetUserAsync(User);
         if (user == null)
         {
             return NotFound("Пользователь не найден");
         }

         if (user.Balance < model.Amount)
         {
             ModelState.AddModelError("Amount", "Недостаточно средств для оплаты");
             var serviceProviders = await _context.ServiceProviders.ToListAsync();
             ViewBag.ServiceProviders = serviceProviders;
             return View(model);
         }

         var serviceUser = await _context.ServiceUsers.Include(u => u.ServiceProvider)
             .FirstOrDefaultAsync(su => su.ServiceProviderId == model.ServiceProviderId && su.Identifier == model.Identifier);

         if (serviceUser == null)
         {
             ModelState.AddModelError("Identifier", "Реквизит у поставщика услуг не найден");
             var serviceProviders = await _context.ServiceProviders.ToListAsync();
             ViewBag.ServiceProviders = serviceProviders;
             return View(model);
         }

         user.Balance -= model.Amount;
         serviceUser.Balance += model.Amount;

         Transaction transaction = new Transaction();

         transaction.FromUserId = user.Id;
         transaction.ToUserId = null;
         transaction.Date = DateTime.UtcNow.AddHours(6);
         transaction.Amount = -model.Amount;
         transaction.Description = $"Оплата услуг {serviceUser.Identifier} у {serviceUser.ServiceProvider.Name}";
        

         _context.Transactions.Add(transaction);
         _context.Users.Update(user);
         _context.ServiceUsers.Update(serviceUser);

         await _context.SaveChangesAsync();

         ViewBag.Message = "Оплата успешно выполнена";
         return RedirectToAction("Index");
     }
}