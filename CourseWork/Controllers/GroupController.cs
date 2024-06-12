using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourseWork.Models;
using Microsoft.AspNetCore.Identity;

namespace CourseWork.Controllers
{
    public class GroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Group
        public async Task<IActionResult> Index()
        {
            var currentUser = await _context.Users
                                            .Include(u => u.Group)
                                            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser == null || currentUser.Group == null)
            {
                // Если текущий пользователь не найден или у него нет группы, можно вернуть сообщение или другое действие
                return View("NoGroup"); // Например, представление с сообщением "Группа не найдена"
            }

            var userGroup = currentUser.Group;
            return View(userGroup);
        }

        // GET: Group/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // GET: Group/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Group/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupId,GroupName,Balance")] Group @group)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, создавал ли пользователь уже группу
                var currentUser = await _context.Users
                                                .Include(u => u.Group)
                                                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (currentUser == null)
                {
                    return NotFound(); // Если пользователь не найден (что маловероятно при аутентификации), можно вернуть ошибку
                }

                if (currentUser.Group != null)
                {
                    // У пользователя уже есть группа, не разрешаем создавать новую
                    ModelState.AddModelError(string.Empty, "You already have a group.");
                    return View(@group);
                }

                // Добавляем текущего пользователя в создаваемую группу
                @group.Users = new List<ApplicationUser> { currentUser };
                @group.Balance = 0; // Начальный баланс можно задать здесь или на клиентской стороне

                _context.Add(@group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Group/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }
            return View(@group);
        }

        // POST: Group/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,GroupName,Balance")] Group @group)
        {
            if (id != @group.GroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.GroupId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Group/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Group/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            try
            {
                // Находим всех пользователей, у которых GroupId равен удаляемой группе, и устанавливаем GroupId в null
                var usersInGroup = await _context.Users.Where(u => u.GroupId == id).ToListAsync();
                foreach (var user in usersInGroup)
                {
                    user.GroupId = null;
                }

                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Обработка ошибки сохранения изменений
                // Можно добавить логирование или другую обработку ошибок
                ModelState.AddModelError(string.Empty, "Failed to delete the group. " + ex.Message);
                return View(group); // Возвращаем представление с ошибкой
            }
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.GroupId == id);
        }

        public async Task<IActionResult> AddUserToGroup(int id) // Используем 'id'
        {
            Console.WriteLine($"Attempting to find group with ID: {id}"); // Логируем попытку поиска группы

            var group = await _context.Groups
                                      .Include(g => g.Users)
                                      .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null)
            {
                Console.WriteLine($"Group with ID: {id} not found."); // Логируем, если группа не найдена
                return NotFound();
            }

            var usersNotInGroup = await _context.Users
                                                .Where(u => u.GroupId == null)
                                                .ToListAsync();

            ViewBag.UsersNotInGroup = new SelectList(usersNotInGroup, "Id", "UserName");

            return View(group);
        }

        // POST: Group/AddUserToGroup/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToGroup(int id, string userId) // Используем 'id'
        {
            Console.WriteLine($"Attempting to add user with ID: {userId} to group with ID: {id}"); // Логируем попытку добавления пользователя в группу

            var group = await _context.Groups
                                      .Include(g => g.Users)
                                      .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null)
            {
                Console.WriteLine($"Group with ID: {id} not found."); // Логируем, если группа не найдена
                return NotFound();
            }

            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                Console.WriteLine($"User with ID: {userId} not found."); // Логируем, если пользователь не найден
                return NotFound();
            }

            user.GroupId = id;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
