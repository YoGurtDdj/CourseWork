using Microsoft.AspNetCore.Identity;

namespace CourseWork.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
    }
}
