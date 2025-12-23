using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerPlatformu.Data;
using StajyerPlatformu.Models;
using StajyerPlatformu.ViewModels;

namespace StajyerPlatformu.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var interns = await _userManager.GetUsersInRoleAsync("Intern");
        var employers = await _userManager.GetUsersInRoleAsync("Employer");

        // Filter valid/approved users if needed (e.g. IsApproved)
        // For now, showing all as per "everyone sees everyone" request, or maybe filter approved
        var validInterns = interns.Where(u => u.IsApproved).ToList();
        var validEmployers = employers.Where(u => u.IsApproved).ToList();

        // Get active posts count
        using (var scope = HttpContext.RequestServices.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var activePostsCount = await context.InternshipPosts.CountAsync(p => p.IsActive);
            ViewBag.ActivePosts = activePostsCount;
        }

        var model = new HomeIndexViewModel
        {
            Interns = validInterns,
            Employers = validEmployers
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
