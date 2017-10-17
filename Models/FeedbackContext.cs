using Microsoft.EntityFrameworkCore;

namespace ViennaFeedback.Models
{
    public class MvcFeedbackContext : DbContext
    {
        public MvcFeedbackContext (DbContextOptions<MvcFeedbackContext> options)
            : base(options)
        {
        }

        public DbSet<Feedback> Feedback { get; set; }
    }
}