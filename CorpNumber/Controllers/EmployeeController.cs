using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorpNumber.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CorpNumber.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly CorpNumberDbContext _context;

        public EmployeeController(CorpNumberDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> EmployeeIndex(string search)
        {
            var query = _context.Employees
                .Include(e => e.PostNavigation)
                .Include(e => e.DepartmentNavigation)
                .Include(e => e.SectionNavigation)
                .Include(e => e.CodeQuotaNavigation)
                .Where(e => e.Fired != true)
                .AsQueryable();


            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    (e.Surname + " " + e.Firstname + " " + e.Midname).Contains(search));
            }

            var employees = await query.ToListAsync();

            ViewBag.TotalEmployees = employees.Count;
            ViewBag.Search = search;

            return View("EmployeeIndex", employees);
        }
    }
}
