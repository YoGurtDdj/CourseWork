using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models
{
    public class Message
    {
        [Key]
        public string MessageId { get; set; }
        public string MessageText { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
