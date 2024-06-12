using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CourseWork.Controllers;

namespace CourseWork.Models
{
    public class Group : ISubject
    {
        [Key]
        public int GroupId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string GroupName { get; set; }
        public int Balance { get; set; }
        public ICollection<ApplicationUser>? Users { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Transaction>? Transactions { get; set; }

        private List<IObserver> _observers = new List<IObserver>();

        public void Attach(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify()
        {
            foreach (var observer in _observers)
            {
                observer.Update();
            }
        }
    }
}
