using Microsoft.EntityFrameworkCore;
using RMTasks.Models;

namespace RMTasks.Service
{
    public class ReminderService : IReminderService
    {

        private readonly ApplicationDbContext _db;

        public ReminderService(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task Add(Reminder reminder)
        {
            _db.Reminders.Add(reminder);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var ReminderModalToDelete = await _db.Reminders.FindAsync(id);
            if (ReminderModalToDelete != null)
            {
                _db.Reminders.Remove(ReminderModalToDelete);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<Reminder>> GetAll()
        {
            return await _db.Reminders.ToListAsync();
        }

        public async Task<Reminder> Get(int id)
        {
            return await _db.Reminders.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Reminder>> GetDue(DateTime dueDate)
        {
            return await _db.Reminders.Where(r => r.DateTime <= dueDate && !r.IsSent).ToListAsync();
        }

        public async Task Update(Reminder reminder)
        {
            _db.Reminders.Update(reminder);
            await _db.SaveChangesAsync();
        }
    }
}
