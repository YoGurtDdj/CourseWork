using CourseWork.Models;

namespace CourseWork.Controllers
{
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify();
    }

    public interface IObserver
    {
        void Update();
    }

    public class GroupBalanceObserver : IObserver
    {
        private readonly Group _group;

        public GroupBalanceObserver(Group group)
        {
            _group = group;
            _group.Attach(this); // Присоединяем наблюдателя к группе
        }

        public void Update()
        {
            Console.WriteLine($"Group balance updated for group: {_group.GroupName}");
            // Добавьте логику для обновления UI или отправки уведомлений пользователям
        }
    }

}
