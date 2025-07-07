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
            var departments = _context.Departments.OrderBy(d => d.DepartmentName).ToList();

            var model = _context.Sections
                .Include(s => s.DepartmentNavigation)
                .Select(s => new SectionViewModel
                {
                    CodeSection = s.CodeSection,
                    Section = s.Section,
                    SectionCh = s.SectionCh,
                    CodeDepartment = s.Department,
                    DepartmentName = s.DepartmentNavigation != null
                        ? s.DepartmentNavigation.DepartmentName + " " + s.DepartmentNavigation.DepartmentCh
                        : "-",
                    Actuality = s.Actuality
                })
                .ToList();

            ViewBag.Departments = departments;
            return View(model);
        }

        //фильтр таблицы секций
        [HttpGet]
        public IActionResult FilterSections(string searchText, int? departmentId)
        {
            var query = _context.Sections.Include(s => s.DepartmentNavigation).AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(s => s.Section.Contains(searchText));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Department == departmentId);
            }

            var model = query.Select(s => new SectionViewModel
            {
                CodeSection = s.CodeSection,
                Section = s.Section,
                SectionCh = s.SectionCh,
                DepartmentName = s.DepartmentNavigation != null
                    ? s.DepartmentNavigation.DepartmentName + " " + s.DepartmentNavigation.DepartmentCh
                    : "-",
                Actuality = s.Actuality
            }).ToList();

            return PartialView("_SectionTable", model);
        }

    }
}
