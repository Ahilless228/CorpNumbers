using CorpNumber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorpNumber.Controllers
{
    public class SectionController : Controller
    {
        private readonly CorpNumberDbContext _context;

        public SectionController(CorpNumberDbContext context)
        {
            _context = context;
        }

        public IActionResult SectionIndex()
        {
            var model = _context.Sections
                .Include(s => s.DepartmentNavigation)
                .Select(s => new SectionViewModel
                {
                    CodeSection = s.CodeSection,
                    Section = s.Section,
                    SectionCh = s.SectionCh,
                    DepartmentName = s.DepartmentNavigation != null
                        ? s.DepartmentNavigation.DepartmentName + " " + s.DepartmentNavigation.DepartmentCh
                        : "-",
                    Actuality = s.Actuality
                })
                .ToList();

            return View(model); // не забудь передать модель
        }
    }
}
