using Cw9.Models;
using Cw9.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Cw9.Controllers;

public class AccountController : Controller
{
    public WalletContext _context;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, WalletContext context)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("admin"))
            {
                var users = await _context.Users.Where(u => u.Id > 1).ToListAsync();
                return View(users);
            }
        }

        return NotFound();
    }
    
    [Authorize]
    public async Task<IActionResult> Profile(int? userId)
    {
        if (User.IsInRole("admin"))
        {
            if (userId != null)
            {
                var getUser = _context.Users.FirstOrDefault(u => u.Id == userId);
                return View(getUser);
            }
        }
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        return View(user);
    }
    
    
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user != null && (userId == id || User.IsInRole("admin")))
        {
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PersonalAccount = user.PersonalAccount,
                IsNeededNewPersonalAccount = false
            };
            return View(model);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (_context.Users.Any(u => u.Email == model.Email && u.Id != model.Id))
        {
            ModelState.AddModelError(string.Empty, "Логин уже существует");
            return View(model);
        }
        User? identityUser = await _userManager.FindByIdAsync(Convert.ToString(model.Id));
        if (identityUser != null)
        {
            if (model.IsNeededNewPersonalAccount)
            {
                Random random = new Random();
                string accountNumber;
                do
                {
                    accountNumber = Convert.ToString(random.Next(100000, 999999));
                } while (_context.Users.Any(u => u.PersonalAccount.Equals(accountNumber)));

                identityUser.PersonalAccount = accountNumber;
            }
            if (ModelState.IsValid)
            {
                identityUser.Email = model.Email;
                identityUser.UserName = model.UserName;
                identityUser.PasswordHash = model.Password != null
                    ? _userManager.PasswordHasher.HashPassword(identityUser, model.Password)
                    : identityUser.PasswordHash;
                var result = await _userManager.UpdateAsync(identityUser);
                if (result.Succeeded)
                {
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        return View(model);
    }
    
    [Authorize]
    [HttpGet]
    [ActionName("Delete")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ConfirmDelete(int? id)
    {
        if (id != null)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(p => p.Id == id);
            if (user != null && User.IsInRole("admin"))
            {
                return View(user);
            }
        }
        return NotFound();
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id != null)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }
    
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
       return View(new LoginViewModel(){ReturnUrl = returnUrl});
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User? user = await _userManager.FindByEmailAsync(model.EmailOrLogin);
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.PersonalAccount.Equals(model.EmailOrLogin));
            }
            if (user != null)
            {
                
                SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");// change redirect
                }
            }
            ModelState.AddModelError("", "Invalid email or password");
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        Random random = new Random();
        string accountNumber;
        do
        {
            accountNumber = Convert.ToString(random.Next(100000, 999999));
        } while (_context.Users.Any(u => u.PersonalAccount.Equals(accountNumber)));
        return View(new RegisterViewModel(){PersonalAccount = accountNumber});
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (_context.Users.Any(u => u.Email == model.Email) || 
            _context.Users.Any(u => u.UserName == model.UserName))
        {
            ModelState.AddModelError("UserName", "Логин или имя уже существует");
            return View(model);
        }
        if (ModelState.IsValid)
        {
            
            User user = new User()
            {
                UserName = model.UserName,
                Balance = 100000,
                Email = model.Email,
                PersonalAccount = model.PersonalAccount
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
                if (!User.IsInRole("admin"))
                {
                    await _signInManager.SignInAsync(user, false);
                }
                return RedirectToAction("Index", "Home");// change redirect 
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> AccessDenied(string returnUrl = null)
    {
        return RedirectToAction("Login", new { returnUrl = returnUrl });
    }
}