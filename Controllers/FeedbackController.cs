using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace ViennaFeedback.Controllers 
{
    public class FeedbackController : Controller 
    {
        // GET: /Feedback
        // Show a feedback list
        public IActionResult Index()
        {
            return View();
        }
        // GET: /Feedback/Welcome
        public string Welcome(string name, int numTimes = 1)
        {
            return HtmlEncoder.Default.Encode($"Hello {name}, NumTimes is: {numTimes}");
        }
    }
}