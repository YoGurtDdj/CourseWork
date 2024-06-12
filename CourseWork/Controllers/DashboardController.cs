﻿using CourseWork.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace CourseWork.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Category.UserId == user.Id && y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();

            int TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = $"{TotalIncome.ToString("N0")} ₸";

            int TotalExpense = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = $"{TotalExpense.ToString("N0")} ₸";
            Console.WriteLine(ViewBag.TotalExpense);

            int Balance = TotalIncome - TotalExpense;
            ViewBag.Balance = $"{Balance.ToString("N0")} ₸";

            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount),
                }).ToList();

            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount),
                }).ToList();

            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            var splineChartData = from day in Last7Days
                                  join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                  from income in dayIncomeJoined.DefaultIfEmpty()
                                  join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                  from expense in expenseJoined.DefaultIfEmpty()
                                  select new
                                  {
                                      day = day,
                                      income = income == null ? 0 : income.income,
                                      expense = expense == null ? 0 : expense.expense,
                                  };

            ViewBag.SplineChartData = JsonConvert.SerializeObject(splineChartData.ToList());
            ViewBag.TotalIncome = IncomeSummary.Sum(x => x.income);
            ViewBag.TotalExpense = ExpenseSummary.Sum(x => x.expense);
            ViewBag.Balance = ViewBag.TotalIncome - ViewBag.TotalExpense;

            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .Where(t => t.Category.UserId == user.Id)
                .OrderByDescending(j => j.Date)
                .Take(6)
                .ToListAsync();

            return View();
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}
