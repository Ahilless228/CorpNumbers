using CorpNumber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CorpNumber.Controllers
{
    public class RoutersController : Controller
    {
        private readonly CorpNumberDbContext _context;

        public RoutersController(CorpNumberDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> RoutersIndex()
        {
            var routers = await _context.Routers
                .Include(r => r.NumberPhone)
                    .ThenInclude(p => p.AccountNavigation)
                .Include(r => r.NumberPhone)
                    .ThenInclude(p => p.CodeOwnerNavigation)
                        .ThenInclude(o => o.CategoryNavigation)
                .ToListAsync();

            return View("RoutersIndex", routers);
        }
    }
}