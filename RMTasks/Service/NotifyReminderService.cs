namespace RMTasks.Service
{
    public class NotifyReminderService: BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public NotifyReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var remindersDue = await reminderRepository.GetDue(DateTime.Now);

                    foreach (var reminder in remindersDue)
                    {
                        var messageBody = $"<h1>Reminder: {reminder.Title}</h1>" +$"<p>Title: {reminder.Title}</p>" + $"<p>Time:{reminder.DateTime}</p>";

                        await emailService.SendEmailAsync(reminder.Email, "Reminder: " + reminder.Title, messageBody);
                        reminder.IsSent = true;
                        await reminderRepository.Update(reminder);
                    }
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                }
            }
        }
    }
}
