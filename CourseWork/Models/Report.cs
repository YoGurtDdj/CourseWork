using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models
{
    public class Report
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalIncome { get; set; }
        public int TotalExpense { get; set; }
        public int Balance { get; set; }

        public string Type { get; set; }
    }
}
