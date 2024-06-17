using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourseWork.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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
                return View(); 
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
                var currentUser = await _context.Users
                                                .Include(u => u.Group)
                                                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (currentUser == null)
                {
                    return NotFound(); 
                }
                currentUser.IsGroupCreator = true;
                @group.Users = new List<ApplicationUser> { currentUser };

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

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            var currentUser = await _context.Users
                                            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser == null || !currentUser.IsGroupCreator || currentUser.GroupId != group.GroupId)
            {
                return Forbid();
            }

            return View(group);
        }

        // POST: Group/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,GroupName,Description")] Group group)
        {
            if (id != group.GroupId)
            {
                return NotFound();
            }

            var currentUser = await _context.Users
                                            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser == null || !currentUser.IsGroupCreator || currentUser.GroupId != group.GroupId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(group.GroupId))
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
            return View(group);
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
                // Find categories related to the group and remove them
                var categoriesToRemove = await _context.Categories.Where(c => c.GroupId == id).ToListAsync();
                _context.Categories.RemoveRange(categoriesToRemove);

                // Remove users from the group (if needed)
                var usersInGroup = await _context.Users.Where(u => u.GroupId == id).ToListAsync();
                foreach (var user in usersInGroup)
                {
                    user.GroupId = null;
                    user.IsGroupCreator = false;
                }

                // Remove the group itself
                _context.Groups.Remove(group);

                // Save changes to the database
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Handle exceptions if necessary
                ModelState.AddModelError(string.Empty, "Failed to delete the group. " + ex.Message);
                return View(group);
            }
        }


        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.GroupId == id);
        }

        public async Task<IActionResult> AddUserToGroup(int id) 
        {
            var group = await _context.Groups
        .Include(g => g.Users)
        .Include(g => g.Categories)
        .Include(g => g.Transactions)
        .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null)
            {
                return NotFound();
            }

            var usersNotInGroup = await _context.Users
                                                .Where(u => u.GroupId == null)
                                                .ToListAsync();

            ViewBag.UsersNotInGroup = usersNotInGroup;

            return View(group);
        }

        // POST: Group/AddUserToGroup/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToGroup(int id, string userId) 
        {
            var group = await _context.Groups
                                      .Include(g => g.Users)
                                      .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            user.GroupId = id;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AddUserToGroup), new { id = id });
        }

        // POST: Group/RemoveUserFromGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromGroup(int id, string userId)
        {
            var group = await _context.Groups
                                      .Include(g => g.Users)
                                      .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null)
            {
                return NotFound();
            }

            var currentUser = await _context.Users
                                            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name && u.IsGroupCreator);

            if (currentUser == null)
            {
                return Forbid();
            }

            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Id == userId && u.GroupId == id);

            if (user == null)
            {
                return NotFound();
            }

            user.GroupId = null;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AddUserToGroup), new { id = id });
        }

        // POST: Group/LeaveGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveGroup()
        {
            var currentUser = await _context.Users
                                            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser == null)
            {
                return NotFound();
            }

            if (currentUser.GroupId == null)
            {
                ModelState.AddModelError(string.Empty, "You are not part of any group.");
                return RedirectToAction(nameof(Index));
            }

            currentUser.GroupId = null;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Group/AddCategory/5
        public IActionResult AddCategory(int groupId, int categoryId = 0)
        {
            if (categoryId == 0)
            {
                return View(new Category { GroupId = groupId });
            }
            else
            {
                var category = _context.Categories.FirstOrDefault(c => c.CategoryId == categoryId && c.GroupId == groupId);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
        }

        // POST: Group/AddCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int groupId, Category category)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                category.GroupId = groupId;
                if (category.CategoryId == 0)
                {
                    _context.Add(category);
                    group.Categories.Add(category);
                }
                else
                {
                    _context.Update(category);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AddUserToGroup), new { id = groupId });
            }
            return View(category);
        }

        // GET: Group/AddTransaction/5
        public IActionResult AddTransaction(int groupId, int transactionId = 0)
        {
            PopulateCategories(groupId);
            if (transactionId == 0)
                return View(new Transaction { GroupId = groupId});
            else
                return View(_context.Transactions.Find(transactionId));
        }

        // POST: Group/AddTransaction/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTransaction(int groupId, Transaction transaction)
        {
            var group = await _context.Groups.FindAsync(groupId);

            if(group == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                transaction.GroupId = groupId;
                if (transaction.TransactionId == 0)
                {
                    _context.Add(transaction);
                    group.Transactions.Add(transaction);
                }
                else
                    _context.Update(transaction);
                var users = await _context.Users
            .Where(u => u.GroupId == groupId).ToListAsync();

                Notifier notifier = new GroupTransactionNotifier(_context);
                notifier = new ConsoleNotifierDecorator($"В вашей группе была произведена транзакция на сумму {transaction.Amount}", _context, notifier);
                notifier.Notify(users);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AddUserToGroup), new { id = groupId });
            }
            else
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }
            PopulateCategories(groupId);
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int groupId, int id)
        {
            var group = await _context.Groups.FindAsync(groupId);
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            group.Categories.Remove(category);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AddUserToGroup), new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTransaction(int groupId, int transactionId)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AddUserToGroup), new { id = groupId });
        }

        public async void PopulateCategories(int groupId)
        {
            var CategoryCollection = _context.Categories
                .Where(c => c.GroupId == groupId)
                .ToList();

            Category DefaultCategory = new Category() { CategoryId = 0, Title = "Choose a category" };
            CategoryCollection.Insert(0, DefaultCategory);
            ViewBag.Categories = CategoryCollection;
        }
    }
}
