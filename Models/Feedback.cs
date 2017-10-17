using System;
using System.ComponentModel.DataAnnotations;

namespace ViennaFeedback.Models
{
    public class Feedback
    {
        [Key]
        public DateTime eventTime {get; set;}
        public string appVersion { get; set;}
        public string clientIP { get; set; }
        public string message { get; set; }
        public string feedbackType { get; set; }
        public string email { get; set; }
        public string screenshot { get; set; }
    }
}