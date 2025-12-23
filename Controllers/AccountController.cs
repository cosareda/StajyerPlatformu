using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerPlatformu.Models;
using StajyerPlatformu.ViewModels;

namespace StajyerPlatformu.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign Role
                    string role = "Intern";
                    if (!string.IsNullOrEmpty(model.UserRole) && (model.UserRole == "Intern" || model.UserRole == "Employer"))
                    {
                        role = model.UserRole;
                    }
                    await _userManager.AddToRoleAsync(user, role);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    // Redirect based on role
                    if (role == "Intern")
                    {
                        return RedirectToAction("Index", "Intern");
                    }
                    else if (role == "Employer")
                    {
                         return RedirectToAction("Index", "Employer");
                    }

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    
                    if (!user.IsApproved)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Hesabınız henüz onaylanmadı. Lütfen yönetici onayını bekleyin.");
                        return View(model);
                    }

                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (roles.Contains("Intern"))
                    {
                        return RedirectToAction("Index", "Intern");
                    }
                    else if (roles.Contains("Employer"))
                    {
                        return RedirectToAction("Index", "Employer");
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email adresi gereklidir.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Güvenlik için: Kullanıcı bulunamasa bile başarı mesajı göster
                TempData["Message"] = "Eğer bu email adresi sistemde kayıtlıysa, şifre sıfırlama linki gönderildi.";
                return RedirectToAction("Login");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, Request.Scheme);

            // Gerçek uygulamada email gönderilir, şimdilik token'ı log'a yazıyoruz
            // TODO: Email servisi entegre edilmeli
            TempData["ResetToken"] = token;
            TempData["ResetUserId"] = user.Id;
            TempData["Message"] = $"Şifre sıfırlama linki: {callbackUrl} (Geliştirme modu - gerçek uygulamada email gönderilir)";
            
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string token, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                ModelState.AddModelError("", "Şifreler eşleşmiyor.");
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (result.Succeeded)
            {
                TempData["Message"] = "Şifreniz başarıyla sıfırlandı. Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin")) return RedirectToAction("Index", "Admin");
            if (roles.Contains("Employer")) return RedirectToAction("Index", "Employer");
            if (roles.Contains("Intern")) return RedirectToAction("Index", "Intern");
            
            return RedirectToAction("Index", "Home");
        }
    }
}
