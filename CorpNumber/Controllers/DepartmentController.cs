using Microsoft.AspNetCore.Mvc;
using CorpNumber.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace CorpNumber.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly CorpNumberDbContext _context;
        public DepartmentController(CorpNumberDbContext context)
        {
            _context = context;
        }
        public IActionResult DepartmentIndex()
        {
            var model = _context.Departments
                .Select(d => new Department
                {
                    CodeDepartment = d.CodeDepartment,
                    DepartmentName = d.DepartmentName,
                    DepartmentCh = d.DepartmentCh,
                    Actuality = d.Actuality
                })
                .OrderBy(d => d.DepartmentName)
                .ToList();

            return View(model);
        }


        //метод поиска по названию категории
        [HttpGet]
        public IActionResult FilterDepartment(string searchText)
        {
            var query = _context.Departments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c => c.DepartmentName != null && c.DepartmentName.Contains(searchText));
            }

            var model = query.OrderBy(c => c.DepartmentName).ToList();
            return PartialView("_DepartmentTable", model);
        }

        [HttpGet]
        public IActionResult GetDepartmentById(int id)
        {
            var department = _context.Departments.FirstOrDefault(c => c.CodeDepartment == id);
            if (department == null) return NotFound();

            return Json
                (new { codeDepartment = department.CodeDepartment,
                    departmentName = department.DepartmentName,
                    departmentCh = department.DepartmentCh, actuality = department.Actuality });
        }
        //метод сохранения и обновления категории поста
        [HttpPost]
        public IActionResult SaveDepartment(Department model)
        {
            if (model.CodeDepartment == 0)
            {
                _context.Departments.Add(model);
            }
            else
            {
                var existing = _context.Departments.Find(model.CodeDepartment);
                if (existing == null) return NotFound();

                existing.DepartmentName = model.DepartmentName;
                existing.DepartmentCh = model.DepartmentCh;
                existing.Actuality = model.Actuality;
            }

            _context.SaveChanges();
            return Ok();
        }


    }
}
