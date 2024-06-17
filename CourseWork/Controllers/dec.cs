using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Controllers
{
    public abstract class Notifier
    {
        public readonly ApplicationDbContext _context;
        public Notifier(string n, ApplicationDbContext context)
        {
            this.Message = n;
            _context = context;
        }
        public string Message { get; set; }
        public abstract void Notify(ICollection<ApplicationUser> users);
    }
    class GroupTransactionNotifier : Notifier
    {
        public GroupTransactionNotifier(ApplicationDbContext context) : base("Произведена транзакция", context)
        { }
        public override void Notify(ICollection<ApplicationUser> users)
        {
            if(users != null)
            {
                foreach (ApplicationUser user in users)
                {
                    Console.WriteLine(Message);
                }
            }
            else
            {
                Console.WriteLine("не работает");
            }
        }
    }
    abstract class NotifierDecorator : Notifier
    {
        protected Notifier notifier;
        public NotifierDecorator(string n, ApplicationDbContext context, Notifier notifier) : base(n, context)
        {
            this.notifier = notifier;
        }
    }
    class ConsoleNotifierDecorator : NotifierDecorator
    {
        public ConsoleNotifierDecorator(string text, ApplicationDbContext _context, Notifier not) : base(text, _context, not)
        { }
        public override void Notify(ICollection<ApplicationUser> users)
        {
            if (users != null && users.Any())
            {
                foreach (var user in users)
                {
                    var message = new Message
                    {
                        MessageId = Guid.NewGuid().ToString(), // Генерация уникального идентификатора
                        MessageText = Message,
                        Date = DateTime.Now,
                        UserId = user.Id // Присвоение идентификатора пользователя
                    };

                    _context.Messages.Add(message); // Добавление сообщения в DbSet<Message>
                }

                _context.SaveChanges(); // Сохранение изменений в базе данных
            }
            else
            {
                Console.WriteLine("Коллекция пользователей пуста или равна null");
            }
        }
    }
}
