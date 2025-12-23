using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerPlatformu.Models;
using StajyerPlatformu.Services;

namespace StajyerPlatformu.Controllers
{
    [Authorize(Roles = "Intern")]
    public class InternController : Controller
    {
        // YENİ: Service katmanı kullanımı
        private readonly IInternService _internService;
        private readonly UserManager<AppUser> _userManager;
        
        // ESKİ: Direkt DbContext kullanımı (yedek - isterseniz silebilirsiniz)
        // private readonly AppDbContext _context;

        public InternController(IInternService internService, UserManager<AppUser> userManager)
        {
            _internService = internService;
            _userManager = userManager;
        }

        // DASHBOARD: Shows Profile + Stats
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // YENİ: Service kullanımı
            var profile = await _internService.GetProfileAsync(user.Id);
            
            // ESKİ: Direkt DbContext kullanımı (yedek)
            // var profile = await _context.InternProfiles
            //     .Include(p => p.AppUser)
            //     .Include(p => p.Experiences)
            //     .FirstOrDefaultAsync(p => p.AppUserId == user.Id);

            if (profile == null)
            {
                return RedirectToAction("CreateProfile");
            }
            
            return View(profile);
        }

        public async Task<IActionResult> JobSearch(string searchString, string city, string workType, string companyName)
        {
            // YENİ: Service kullanımı
            var posts = await _internService.SearchJobsAsync(searchString, city, workType, companyName);
            
            // Populate Filters for View
            ViewBag.Cities = await _internService.GetCitiesAsync();
            ViewBag.Companies = await _internService.GetCompaniesAsync();
            // WorkType is static enum-like values, can be hardcoded in view or fetched if desired

            return View(posts);
            
            // ESKİ: Direkt DbContext kullanımı (yedek)
            /*
            var posts = _context.InternshipPosts
                .Include(p => p.EmployerProfile)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(s => s.Title.Contains(searchString) || s.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(city))
            {
                posts = posts.Where(p => p.City == city);
            }

            if (!string.IsNullOrEmpty(workType))
            {
                posts = posts.Where(p => p.WorkType == workType);
            }

            if (!string.IsNullOrEmpty(companyName))
            {
                posts = posts.Where(p => p.EmployerProfile != null && p.EmployerProfile.CompanyName.Contains(companyName));
            }
            
            ViewBag.Cities = await _context.InternshipPosts.Select(p => p.City).Distinct().ToListAsync();
            ViewBag.Companies = await _context.InternshipPosts
                .Include(p => p.EmployerProfile)
                .Where(p => p.EmployerProfile != null)
                .Select(p => p.EmployerProfile.CompanyName)
                .Distinct()
                .ToListAsync();

            return View(await posts.ToListAsync());
            */
        }

        [HttpGet]
        public async Task<IActionResult> CreateProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _internService.GetProfileForEditAsync(user.Id);
             
            if(profile != null) 
            {
                return View(profile); 
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile(InternProfile model, IFormFile? resumeFile, IFormFile? photoFile)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // File Upload Logic
            string? profilePhotoPath = null;
            if (photoFile != null && photoFile.Length > 0)
            {
                var originalFileName = Path.GetFileName(photoFile.FileName);
                var extension = Path.GetExtension(originalFileName);
                var newFileName = $"photo_{user.Id}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
                
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                
                var filePath = Path.Combine(folderPath, newFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }
                
                profilePhotoPath = $"/uploads/photos/{newFileName}";
            }

            string? resumePath = null;
            if (resumeFile != null && resumeFile.Length > 0)
            {
                var originalFileName = Path.GetFileName(resumeFile.FileName);
                var extension = Path.GetExtension(originalFileName);
                var newFileName = $"resume_{user.Id}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
                
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                
                var filePath = Path.Combine(folderPath, newFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await resumeFile.CopyToAsync(stream);
                }
                
                resumePath = $"/uploads/resumes/{newFileName}";
            }

            await _internService.CreateOrUpdateProfileAsync(model, user.Id, profilePhotoPath, resumePath);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddExperience(Experience experience)
        {
            var user = await _userManager.GetUserAsync(User);
            var success = await _internService.AddExperienceAsync(experience, user.Id);
            
            if (!success) return RedirectToAction("CreateProfile");
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            await _internService.DeleteExperienceAsync(id);
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<IActionResult> JobDetail(int id)
        {
            var post = await _internService.GetJobDetailAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            // Check if user already applied
            bool hasApplied = false;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    hasApplied = await _internService.CheckIfAppliedAsync(id, user.Id);
                }
            }

            ViewBag.HasApplied = hasApplied;
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Apply(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var success = await _internService.ApplyToJobAsync(id, user.Id);

            if (!success)
            {
                // Check if profile exists
                var profile = await _internService.GetProfileAsync(user.Id);
                if (profile == null)
                {
                    return RedirectToAction("CreateProfile");
                }

                // Check if already applied
                var hasApplied = await _internService.CheckIfAppliedAsync(id, user.Id);
                if (hasApplied)
                {
                    TempData["Message"] = "Zaten başvurdunuz.";
                }
                else
                {
                    TempData["Message"] = "Bu ilan artık aktif değil.";
                    return RedirectToAction("JobSearch");
                }
            }
            else
            {
                TempData["Message"] = "Başvurunuz başarıyla alındı.";
            }
            
            return RedirectToAction("JobDetail", new { id });
        }
        
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            var applications = await _internService.GetMyApplicationsAsync(user.Id);
            
            if (applications.Count == 0)
            {
                var profile = await _internService.GetProfileAsync(user.Id);
                if (profile == null) return RedirectToAction("CreateProfile");
            }
                 
            return View(applications);
        }
    }
}
