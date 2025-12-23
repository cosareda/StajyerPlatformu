using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajyerPlatformu.Data;
using StajyerPlatformu.Models;

namespace StajyerPlatformu.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessagesController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Ortak gelen kutusu (hem işveren hem stajyer için)
        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == user.Id)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        // Ortak gönderilenler
        public async Task<IActionResult> Sent()
        {
            var user = await _userManager.GetUserAsync(User);
            var messages = await _context.Messages
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == user.Id)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string receiverId, string? subject = null)
        {
            var receiver = await _context.Users.FindAsync(receiverId);
            if (receiver == null) return NotFound();

            ViewBag.Receiver = receiver;
            ViewBag.Subject = subject;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string receiverId, string? subject, string content)
        {
            var sender = await _userManager.GetUserAsync(User);
            var receiver = await _context.Users.FindAsync(receiverId);
            if (receiver == null) return NotFound();

            if (string.IsNullOrWhiteSpace(content))
            {
                ViewBag.Receiver = receiver;
                ViewBag.Subject = subject;
                ModelState.AddModelError(string.Empty, "Mesaj içeriği boş olamaz.");
                return View();
            }

            var msg = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Subject = subject,
                Content = content,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Mesaj gönderildi.";
            return RedirectToAction("Inbox");
        }
    }
}


