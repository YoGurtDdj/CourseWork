using Microsoft.AspNetCore.Identity;

namespace CourseWork.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? GroupId { get; set; }
        public bool IsGroupCreator { get; set; } = false;
        public Group? Group { get; set; }
    }
}
