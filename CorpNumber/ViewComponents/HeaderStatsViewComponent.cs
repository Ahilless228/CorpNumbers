using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorpNumber.Models;

namespace CorpNumber.ViewComponents
{
    public class HeaderStatsViewComponent : ViewComponent
    {
        private readonly CorpNumberDbContext _context;

        public HeaderStatsViewComponent(CorpNumberDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var birthdayCount = await _context.Employees.CountAsync(e =>
                e.Birthday.HasValue &&
                e.Birthday.Value.Day == DateTime.Today.Day &&
                e.Birthday.Value.Month == DateTime.Today.Month &&
                (!e.Fired.HasValue || e.Fired == false)
            );

            var incompleteOperations = await _context.Operations.CountAsync(op => op.Complete == false);

            var taskTotal = await _context.Tasks.CountAsync(t => t.Complete == false);

            var taskOverdue = await _context.Tasks.CountAsync(t =>
                t.Complete == false &&
                t.TaskDate.HasValue &&
                t.TaskDate < DateTime.Now
            );

            var model = new HeaderStatsViewModel
            {
                BirthdayCount = birthdayCount,
                IncompleteOperations = incompleteOperations,
                TaskOverdue = taskOverdue,
                TaskTotal = taskTotal
            };

            return View(model);
        }

    }
}
