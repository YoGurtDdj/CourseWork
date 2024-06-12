namespace CourseWork.Models
{
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalIncome { get; set; }
        public int TotalExpense { get; set; }
        public int Balance { get; set; }
        public string ReportType { get; set; }
        public string UserId { get; set; }

        public string PeriodInfo { get; set; }
        public List<DailyTransactionSummary>? DailySummaries { get; set; }

        public class DailyTransactionSummary
        {
            public DateTime Date { get; set; }
            public int TotalIncome { get; set; }
            public int TotalExpense { get; set; }
            public int Balance { get; set; }
        }
    }
}
