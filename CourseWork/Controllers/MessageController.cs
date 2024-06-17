using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace CourseWork.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MessageController/Index
        public async Task<IActionResult> Index()
        {
            // Получаем текущего пользователя
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Извлекаем сообщения текущего пользователя
            var messages = await _context.Messages
                                         .Where(m => m.UserId == userId)
                                         .Include(m => m.User) // Опционально: если нужна информация о пользователе
                                         .ToListAsync();

            return View(messages);
        }
    }
}
