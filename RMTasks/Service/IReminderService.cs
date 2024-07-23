using RMTasks.Models;

namespace RMTasks.Service
{
    public interface IReminderService
    {
        Task<List<Reminder>> GetAll();
        Task<Reminder> Get(int id);
        Task<List<Reminder>> GetDue(DateTime dueDate);
        Task Add(Reminder reminder);
        Task Update(Reminder reminder);
        Task Delete(int id);
    }
}
