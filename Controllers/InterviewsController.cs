using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerPlatformu.Data;
using StajyerPlatformu.Models;

namespace StajyerPlatformu.Controllers
{
    [Authorize]
    public class InterviewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public InterviewsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // EMPLOYER: Show all interviews for this employer
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> EmployerList()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);
            if (profile == null) return RedirectToAction("CreateProfile", "Employer");

            var interviews = await _context.Interviews
                .Include(i => i.InternProfile).ThenInclude(p => p.AppUser)
                .Include(i => i.InternshipPost)
                .Where(i => i.EmployerProfileId == profile.Id)
                .OrderByDescending(i => i.ScheduledAt)
                .ToListAsync();

            return View("EmployerList", interviews);
        }

        // INTERN: Show interviews for logged in intern
        [Authorize(Roles = "Intern")]
        public async Task<IActionResult> InternList()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.InternProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);
            if (profile == null) return RedirectToAction("CreateProfile", "Intern");

            var interviews = await _context.Interviews
                .Include(i => i.EmployerProfile).ThenInclude(e => e.AppUser)
                .Include(i => i.InternshipPost)
                .Where(i => i.InternProfileId == profile.Id)
                .OrderByDescending(i => i.ScheduledAt)
                .ToListAsync();

            return View("InternList", interviews);
        }

        // EMPLOYER: Schedule interview for an intern and post
        [Authorize(Roles = "Employer")]
        [HttpGet]
        public async Task<IActionResult> Schedule(int internProfileId, int postId)
        {
            var post = await _context.InternshipPosts
                .Include(p => p.EmployerProfile)
                .FirstOrDefaultAsync(p => p.Id == postId);
            var intern = await _context.InternProfiles
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync(p => p.Id == internProfileId);

            if (post == null || intern == null) return NotFound();

            ViewBag.Post = post;
            ViewBag.Intern = intern;

            return View();
        }

        [Authorize(Roles = "Employer")]
        [HttpPost]
        public async Task<IActionResult> Schedule(int internProfileId, int postId, DateTime scheduledAt, string location, string? note)
        {
            var user = await _userManager.GetUserAsync(User);
            var employerProfile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);
            if (employerProfile == null) return RedirectToAction("CreateProfile", "Employer");

            var intern = await _context.InternProfiles.FirstOrDefaultAsync(p => p.Id == internProfileId);
            var post = await _context.InternshipPosts.FirstOrDefaultAsync(p => p.Id == postId);
            if (intern == null || post == null) return NotFound();

            if (scheduledAt == default)
            {
                TempData["Message"] = "Lütfen geçerli bir tarih ve saat seçiniz.";
                return RedirectToAction("Schedule", new { internProfileId, postId });
            }

            var interview = new Interview
            {
                EmployerProfileId = employerProfile.Id,
                InternProfileId = intern.Id,
                InternshipPostId = post.Id,
                ScheduledAt = scheduledAt,
                Location = string.IsNullOrWhiteSpace(location) ? "Online" : location,
                Note = note,
                Status = InterviewStatus.Planned
            };

            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Mülakat planlandı.";
            return RedirectToAction("EmployerList");
        }

        [Authorize(Roles = "Employer")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, InterviewStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);
            if (profile == null) return RedirectToAction("CreateProfile", "Employer");

            var interview = await _context.Interviews.FirstOrDefaultAsync(i => i.Id == id && i.EmployerProfileId == profile.Id);
            if (interview == null) return NotFound();

            interview.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("EmployerList");
        }
    }
}


