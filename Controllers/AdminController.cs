using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerPlatformu.Data;
using StajyerPlatformu.Models;
using StajyerPlatformu.ViewModels;
using System.IO;
using OfficeOpenXml;

namespace StajyerPlatformu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var posts = await _context.InternshipPosts.Include(p => p.EmployerProfile).ToListAsync();
            var applications = await _context.Applications.CountAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                TotalInterns = (await _userManager.GetUsersInRoleAsync("Intern")).Count,
                TotalEmployers = (await _userManager.GetUsersInRoleAsync("Employer")).Count,
                PendingApprovals = users.Count(u => !u.IsApproved),
                TotalJobs = posts.Count,
                TotalApplications = applications,
                PendingUsers = users.Where(u => !u.IsApproved).OrderByDescending(u => u.CreatedAt).ToList(),
                RecentJobs = posts.OrderByDescending(p => p.CreatedDate).Take(5).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsApproved = true;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var post = await _context.InternshipPosts.FindAsync(id);
            if (post != null)
            {
                _context.InternshipPosts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logs()
        {
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logDirectory)) return View(new List<string>());

            var logFile = Directory.GetFiles(logDirectory, "log-*.txt")
                                   .OrderByDescending(f => f)
                                   .FirstOrDefault();

            if (logFile == null) return View(new List<string>());

            var lines = await System.IO.File.ReadAllLinesAsync(logFile);
            return View(lines.Reverse().Take(100).ToList());
        }

        public async Task<IActionResult> ExportUsersExcel()
        {
            var users = await _userManager.Users.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");
                worksheet.Cells[1, 1].Value = "Ad Soyad";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Onay Durumu";
                worksheet.Cells[1, 4].Value = "Kayit Tarihi";

                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = $"{user.FirstName} {user.LastName}";
                    worksheet.Cells[row, 2].Value = user.Email;
                    worksheet.Cells[row, 3].Value = user.IsApproved ? "Onayli" : "Onay Bekliyor";
                    worksheet.Cells[row, 4].Value = user.CreatedAt.ToShortDateString();
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"Kullanicilar_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public async Task<IActionResult> ExportJobsExcel()
        {
            var posts = await _context.InternshipPosts.Include(p => p.EmployerProfile).ThenInclude(e => e.AppUser).ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("JobPosts");
                worksheet.Cells[1, 1].Value = "Baslik";
                worksheet.Cells[1, 2].Value = "Isveren";
                worksheet.Cells[1, 3].Value = "Olusturulma Tarihi";
                worksheet.Cells[1, 4].Value = "Bitis Tarihi";
                worksheet.Cells[1, 5].Value = "Durum";

                int row = 2;
                foreach (var post in posts)
                {
                    worksheet.Cells[row, 1].Value = post.Title;
                    worksheet.Cells[row, 2].Value = post.EmployerProfile?.AppUser?.FirstName ?? "Bilinmiyor";
                    worksheet.Cells[row, 3].Value = post.CreatedDate.ToShortDateString();
                    worksheet.Cells[row, 4].Value = post.Deadline.ToShortDateString();
                    worksheet.Cells[row, 5].Value = post.IsActive ? "Aktif" : "Pasif";
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"Ilanlar_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public async Task<IActionResult> Applications()
        {
            var applications = await _context.Applications
                .Include(a => a.InternshipPost)
                .ThenInclude(p => p.EmployerProfile)
                .ThenInclude(e => e.AppUser)
                .Include(a => a.InternProfile)
                .ThenInclude(i => i.AppUser)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return View(applications);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var usersWithRoles = new List<dynamic>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new
                {
                    User = user,
                    Roles = roles
                });
            }

            ViewBag.UsersWithRoles = usersWithRoles;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, role);
            }
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Posts()
        {
            var posts = await _context.InternshipPosts
                .Include(p => p.EmployerProfile)
                .ThenInclude(e => e.AppUser)
                .Include(p => p.Applications)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.InternshipPosts.FindAsync(id);
            if (post != null)
            {
                _context.InternshipPosts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Posts");
        }

        public async Task<IActionResult> ExportApplicationsExcel()
        {
            var applications = await _context.Applications
                .Include(a => a.InternshipPost)
                .ThenInclude(p => p.EmployerProfile)
                .ThenInclude(e => e.AppUser)
                .Include(a => a.InternProfile)
                .ThenInclude(i => i.AppUser)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Basvurular");
                worksheet.Cells[1, 1].Value = "İlan";
                worksheet.Cells[1, 2].Value = "İşveren";
                worksheet.Cells[1, 3].Value = "Aday";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Üniversite";
                worksheet.Cells[1, 6].Value = "Bölüm";
                worksheet.Cells[1, 7].Value = "Başvuru Tarihi";
                worksheet.Cells[1, 8].Value = "Durum";

                int row = 2;
                foreach (var app in applications)
                {
                    worksheet.Cells[row, 1].Value = app.InternshipPost.Title;
                    worksheet.Cells[row, 2].Value = app.InternshipPost.EmployerProfile?.CompanyName ?? "Bilinmiyor";
                    worksheet.Cells[row, 3].Value = $"{app.InternProfile.AppUser.FirstName} {app.InternProfile.AppUser.LastName}";
                    worksheet.Cells[row, 4].Value = app.InternProfile.AppUser.Email;
                    worksheet.Cells[row, 5].Value = app.InternProfile.University;
                    worksheet.Cells[row, 6].Value = app.InternProfile.Department;
                    worksheet.Cells[row, 7].Value = app.ApplicationDate.ToShortDateString();
                    worksheet.Cells[row, 8].Value = app.Status.ToString();
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"TumBasvurular_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public async Task<IActionResult> ExportApplicationsPdf()
        {
            var applications = await _context.Applications
                .Include(a => a.InternshipPost)
                .ThenInclude(p => p.EmployerProfile)
                .ThenInclude(e => e.AppUser)
                .Include(a => a.InternProfile)
                .ThenInclude(i => i.AppUser)
                .ToListAsync();

            using (var stream = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(stream);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                document.Add(new iText.Layout.Element.Paragraph("Tüm Başvurular Raporu").SetFontSize(20).SetBold());
                document.Add(new iText.Layout.Element.Paragraph($"Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}").SetFontSize(12));
                document.Add(new iText.Layout.Element.Paragraph(" "));
                
                var table = new iText.Layout.Element.Table(6, true);
                table.AddHeaderCell("İlan");
                table.AddHeaderCell("İşveren");
                table.AddHeaderCell("Aday");
                table.AddHeaderCell("Üniversite");
                table.AddHeaderCell("Başvuru Tarihi");
                table.AddHeaderCell("Durum");

                foreach (var app in applications)
                {
                    table.AddCell(app.InternshipPost.Title);
                    table.AddCell(app.InternshipPost.EmployerProfile?.CompanyName ?? "Bilinmiyor");
                    table.AddCell($"{app.InternProfile.AppUser.FirstName} {app.InternProfile.AppUser.LastName}");
                    table.AddCell(app.InternProfile.University ?? "Belirtilmemiş");
                    table.AddCell(app.ApplicationDate.ToShortDateString());
                    table.AddCell(app.Status.ToString());
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", $"TumBasvurular_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
        }
    }
}
