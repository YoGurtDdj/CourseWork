using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourseWork.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace CourseWork.Controllers
{
    public class ReportController : Controller
    {
        private readonly DailyReportFactory _dailyReportFactory;
        private readonly MonthlyReportFactory _monthlyReportFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ReportController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _dailyReportFactory = new DailyReportFactory();
            _monthlyReportFactory = new MonthlyReportFactory();
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(string reportType)
        {
            var user = await _userManager.GetUserAsync(User);
            DateTime startDate = DateTime.Today.AddDays(-6);
            DateTime endDate = DateTime.Today;

            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Category.UserId == user.Id && y.Date >= startDate && y.Date <= endDate)
                .ToListAsync();

            IReport report;
            ReportViewModel reportViewModel;

            if (reportType == "daily")
            {
                report = _dailyReportFactory.CreateReport();
            }
            else if (reportType == "monthly")
            {
                startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);

                selectedTransactions = await _context.Transactions
                    .Include(x => x.Category)
                    .Where(y => y.Category.UserId == user.Id && y.Date >= startDate && y.Date <= endDate)
                    .ToListAsync();

                report = _monthlyReportFactory.CreateReport();
            }
            else
            {
                return BadRequest("Неизвестный тип отчета");
            }

            reportViewModel = report.Generate(selectedTransactions, startDate, endDate);
            reportViewModel.ReportType = reportType;
            reportViewModel.UserId = user.Id;

            return View("Index", reportViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveReport(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                var report = new Report
                {
                    UserId = reportViewModel.UserId,
                    StartDate = reportViewModel.StartDate,
                    EndDate = reportViewModel.EndDate,
                    TotalIncome = reportViewModel.TotalIncome,
                    TotalExpense = reportViewModel.TotalExpense,
                    Balance = reportViewModel.Balance,
                    Type = reportViewModel.ReportType
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                return RedirectToAction("SavedReports");
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

            return View("Index", reportViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SavedReports()
        {
            var user = await _userManager.GetUserAsync(User);
            var reports = await _context.Reports
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(SavedReports));
        }
    }


    public interface IReport
    {
        ReportViewModel Generate(List<Transaction> transactions, DateTime startDate, DateTime endDate);
    }
    public abstract class ReportFactory
    {
        public abstract IReport CreateReport();
    }
    public class DailyReport : IReport
    {
        public ReportViewModel Generate(List<Transaction> transactions, DateTime startDate, DateTime endDate)
        {
            var dailySummaries = transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .GroupBy(t => t.Date.Date)
                .Select(g => new ReportViewModel.DailyTransactionSummary
                {
                    Date = g.Key,
                    TotalIncome = g.Where(t => t.Category.Type == "Income").Sum(t => t.Amount),
                    TotalExpense = g.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount),
                    Balance = g.Where(t => t.Category.Type == "Income").Sum(t => t.Amount) - g.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount)
                }).ToList();

            var reportViewModel = new ReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalIncome = dailySummaries.Sum(s => s.TotalIncome),
                TotalExpense = dailySummaries.Sum(s => s.TotalExpense),
                Balance = dailySummaries.Sum(s => s.TotalIncome) - dailySummaries.Sum(s => s.TotalExpense),
                DailySummaries = dailySummaries
            };

            return reportViewModel;
        }
    }

    public class MonthlyReport : IReport
    {
        public ReportViewModel Generate(List<Transaction> transactions, DateTime startDate, DateTime endDate)
        {
            int totalIncome = transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate && t.Category.Type == "Income")
                .Sum(t => t.Amount);

            int totalExpense = transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate && t.Category.Type == "Expense")
                .Sum(t => t.Amount);

            var reportViewModel = new ReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = totalIncome - totalExpense,
                DailySummaries = null // Месячный отчет может не требовать ежедневных сводок
            };

            return reportViewModel;
        }
    }

    public class DailyReportFactory : ReportFactory
    {
        public override IReport CreateReport()
        {
            return new DailyReport();
        }
    }
    public class MonthlyReportFactory : ReportFactory
    {
        public override IReport CreateReport()
        {
            return new MonthlyReport();
        }
    }

}
