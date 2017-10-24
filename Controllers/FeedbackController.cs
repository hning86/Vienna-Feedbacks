using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ViennaFeedback.Models;

namespace ViennaFeedback.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly MvcFeedbackContext _context;

        public FeedbackController(MvcFeedbackContext context)
        {
            _context = context;
        }

        // Find all weeks between launch date and now
        private Dictionary<string, string> GetWeeks()
        {
            DateTime launchDate = new DateTime(2017, 9, 25);
            DateTime dt = DateTime.Now;
            DateTime startOfWeek = dt.Date.AddDays(-(int)dt.DayOfWeek);
            DateTime endOfWeek = dt.Date.AddDays(7 - (int)dt.DayOfWeek).AddTicks(-1);
            
            Dictionary<string, string> weeks = new Dictionary<string, string>();
            while (endOfWeek > launchDate) 
            {                
                weeks.Add(startOfWeek.ToString("yyyy-MM-dd"), startOfWeek.ToShortDateString());                
                startOfWeek = startOfWeek.AddDays(-7);
                endOfWeek = endOfWeek.AddDays(-7);
            }
            return weeks;
        }
        [Authorize()]
        // GET: Feedback
        [ServiceFilter(typeof(ViennaFeedback.ClientIPCheckilter))]
        public async Task<IActionResult> Index(DateTime? dt) 
        {
            if (!dt.HasValue)
                dt = DateTime.Now;
            DateTime startOfWeek = dt.Value.Date.AddDays(-(int)dt.Value.DayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);
            
            //return RedirectToAction("Test");
            Dictionary<string, string> weeks = GetWeeks();
            ViewData["Weeks"] = weeks;
            ViewData["CurrentWeek"] = startOfWeek.ToString("yyyy-MM-dd");
            
            return View(await _context.Feedback
                .Where(f => f.eventTime >= startOfWeek && f.eventTime < endOfWeek)
                .OrderByDescending(f => f.eventTime)
                .ToListAsync());    
        }

        public ActionResult Test(){
            Dictionary<string, string> weeks = GetWeeks();
            ViewData["Weeks"] = weeks;
            ViewData["CurrentWeek"] = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        // GET: Feedback/Details/5
        public async Task<IActionResult> Details(DateTime? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedback
                .SingleOrDefaultAsync(m => m.eventTime == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // GET: Feedback/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Feedback/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("eventTime,subject")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(feedback);
        }

        // GET: Feedback/Edit/5
        public async Task<IActionResult> Edit(DateTime? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedback.SingleOrDefaultAsync(m => m.eventTime == id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // POST: Feedback/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DateTime id, [Bind("eventTime,subject")] Feedback feedback)
        {
            if (id != feedback.eventTime)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.eventTime))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(feedback);
        }

        // GET: Feedback/Delete/5
        public async Task<IActionResult> Delete(DateTime? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedback
                .SingleOrDefaultAsync(m => m.eventTime == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // POST: Feedback/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(DateTime id)
        {
            var feedback = await _context.Feedback.SingleOrDefaultAsync(m => m.eventTime == id);
            _context.Feedback.Remove(feedback);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(DateTime id)
        {
            return _context.Feedback.Any(e => e.eventTime == id);
        }
    }
}
