using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CourseWork.Controllers;

namespace CourseWork.Models
{

    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string GroupName { get; set; }
        public int Balance { get; set; }
        public string Description { get; set; }
        public ICollection<ApplicationUser>? Users { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Transaction>? Transactions { get; set; }
    }
}
