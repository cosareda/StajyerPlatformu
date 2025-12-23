using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerPlatformu.Data;
using StajyerPlatformu.Models;
using StajyerPlatformu.ViewModels;
using OfficeOpenXml;

namespace StajyerPlatformu.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public EmployerController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int> GetEmployerInboxCountAsync(string userId)
        {
            return await _context.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }



        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);

            if (profile == null)
            {
                return RedirectToAction("CreateProfile");
            }

            var posts = await _context.InternshipPosts
                .Include(p => p.Applications)
                    .ThenInclude(a => a.InternProfile)
                        .ThenInclude(i => i.AppUser)
                .Where(p => p.EmployerProfileId == profile.Id)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            var viewModel = new EmployerDashboardViewModel
            {
                Profile = profile,
                Posts = posts.Select(p => new InternshipPostWithStats
                {
                    Post = p,
                    ActiveCandidatesCount = p.Applications.Count(a => a.Status != ApplicationStatus.Rejected),
                    AwaitingReviewCount = p.Applications.Count(a => a.Status == ApplicationStatus.Pending),
                    ReviewedCount = p.Applications.Count(a => a.Status == ApplicationStatus.Accepted || a.Status == ApplicationStatus.Rejected),
                    // Contacting and Hired are placeholders as we don't have those statuses yet
                    ContactingCount = 0, 
                    HiredCount = p.Applications.Count(a => a.Status == ApplicationStatus.Accepted) 
                }).ToList(),
                TotalMessages = await GetEmployerInboxCountAsync(user.Id),

            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateProfile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile(EmployerProfile model, IFormFile? logoFile)
        {
            var user = await _userManager.GetUserAsync(User);
            model.AppUserId = user.Id;
            
            // Logo Upload
            if (logoFile != null && logoFile.Length > 0)
            {
                var originalFileName = Path.GetFileName(logoFile.FileName);
                var extension = Path.GetExtension(originalFileName);
                var newFileName = $"logo_{user.Id}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, newFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                model.LogoPath = $"/uploads/logos/{newFileName}";
            }

            // Basic validation check could be added here
            _context.EmployerProfiles.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CreatePost()
        {
             var user = await _userManager.GetUserAsync(User);
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);
            
            if (profile == null) return RedirectToAction("CreateProfile");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(InternshipPost model)
        {
             var user = await _userManager.GetUserAsync(User);
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);
            
            if (profile == null) return RedirectToAction("CreateProfile");

            model.EmployerProfileId = profile.Id;
            model.CreatedDate = DateTime.Now;
            // City, WorkType, Duration are bound automatically from model
            
            _context.InternshipPosts.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        
         public async Task<IActionResult> Applications(int id)
        {
            var post = await _context.InternshipPosts
                .Include(p => p.Applications)
                .ThenInclude(a => a.InternProfile)
                .ThenInclude(i => i.AppUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        public async Task<IActionResult> ExportApplicationsPdf(int id)
        {
            var post = await _context.InternshipPosts
                .Include(p => p.Applications)
                .ThenInclude(a => a.InternProfile)
                .ThenInclude(i => i.AppUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            using (var stream = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(stream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                document.Add(new iText.Layout.Element.Paragraph($"Basvurular - {post.Title}").SetFontSize(20));
                
                var table = new iText.Layout.Element.Table(3, true);
                table.AddCell("Ad Soyad");
                table.AddCell("Universite");
                table.AddCell("Tarih");

                foreach (var app in post.Applications)
                {
                    table.AddCell($"{app.InternProfile.AppUser.FirstName} {app.InternProfile.AppUser.LastName}");
                    table.AddCell($"{app.InternProfile.University}");
                    table.AddCell(app.ApplicationDate.ToShortDateString());
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", $"basvurular_{post.Id}.pdf");
            }
        }

        public async Task<IActionResult> ExportApplicationsExcel(int id)
        {
            var post = await _context.InternshipPosts
                .Include(p => p.Applications)
                .ThenInclude(a => a.InternProfile)
                .ThenInclude(i => i.AppUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Basvurular");
                worksheet.Cells[1, 1].Value = "Ad Soyad";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Universite";
                worksheet.Cells[1, 4].Value = "Bolum";
                worksheet.Cells[1, 5].Value = "Basvuru Tarihi";
                worksheet.Cells[1, 6].Value = "Durum";

                int row = 2;
                foreach (var app in post.Applications)
                {
                    worksheet.Cells[row, 1].Value = $"{app.InternProfile.AppUser.FirstName} {app.InternProfile.AppUser.LastName}";
                    worksheet.Cells[row, 2].Value = app.InternProfile.AppUser.Email;
                    worksheet.Cells[row, 3].Value = app.InternProfile.University;
                    worksheet.Cells[row, 4].Value = app.InternProfile.Department;
                    worksheet.Cells[row, 5].Value = app.ApplicationDate.ToShortDateString();
                    worksheet.Cells[row, 6].Value = app.Status.ToString();
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"Basvurular_{post.Id}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public async Task<IActionResult> CandidateDetail(int id)
        {
            var intern = await _context.InternProfiles
                .Include(i => i.AppUser)
                .Include(i => i.Experiences)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (intern == null) return NotFound();

            return View(intern);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptApplication(int id)
        {
            var application = await _context.Applications
                .Include(a => a.InternshipPost)
                .ThenInclude(p => p.EmployerProfile)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);

            if (profile == null || application.InternshipPost.EmployerProfileId != profile.Id)
            {
                return Forbid();
            }

            application.Status = ApplicationStatus.Accepted;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Başvuru kabul edildi.";
            return RedirectToAction("Applications", new { id = application.InternshipPostId });
        }

        [HttpPost]
        public async Task<IActionResult> RejectApplication(int id)
        {
            var application = await _context.Applications
                .Include(a => a.InternshipPost)
                .ThenInclude(p => p.EmployerProfile)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);

            if (profile == null || application.InternshipPost.EmployerProfileId != profile.Id)
            {
                return Forbid();
            }

            application.Status = ApplicationStatus.Rejected;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Başvuru reddedildi.";
            return RedirectToAction("Applications", new { id = application.InternshipPostId });
        }

        [HttpPost]
        public async Task<IActionResult> SetApplicationInReview(int id)
        {
            var application = await _context.Applications
                .Include(a => a.InternshipPost)
                .ThenInclude(p => p.EmployerProfile)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.AppUserId == user.Id);

            if (profile == null || application.InternshipPost.EmployerProfileId != profile.Id)
            {
                return Forbid();
            }

            application.Status = ApplicationStatus.InReview;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Başvuru durumu 'İnceleniyor' olarak güncellendi.";
            return RedirectToAction("Applications", new { id = application.InternshipPostId });
        }
    }
}
