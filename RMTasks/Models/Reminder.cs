using System.ComponentModel.DataAnnotations;

namespace RMTasks.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public bool IsSent { get; set; }
    }
}
