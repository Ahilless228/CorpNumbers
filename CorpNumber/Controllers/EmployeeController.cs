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

        //выбор сотрудника из таблицы, вся страница
        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetails(int id)
        {
            var emp = await _context.Employees
                .Include(e => e.PostNavigation)
                .Include(e => e.DepartmentNavigation)
                .Include(e => e.SectionNavigation)
                .FirstOrDefaultAsync(e => e.CodeEmployee == id);

            if (emp == null)
                return NotFound();

            string tabnumStr = emp.TabNum?.ToString("D5");
            string photoFileName = $"{tabnumStr}.jpg";

            string rootPath = Directory.GetCurrentDirectory();
            string mainPath = Path.Combine(rootPath, "wwwroot", "Photo", photoFileName);
            string archivePath = Path.Combine(rootPath, "wwwroot", "Photo", "Archive", photoFileName);

            string webPhotoPath;
            if (System.IO.File.Exists(mainPath))
                webPhotoPath = Url.Content($"/Photo/{photoFileName}");
            else if (System.IO.File.Exists(archivePath))
                webPhotoPath = Url.Content($"/Photo/Archive/{photoFileName}");
            else
                webPhotoPath = Url.Content("~/images/default-profile.jpg");

            return Json(new
            {
                fullname = $"{emp.Surname} {emp.Firstname} {emp.Midname}",
                nameCh = emp.NameCh,
                tabnum = tabnumStr,
                inputDate = emp.InputDate?.ToString("dd.MM.yyyy"),
                post = emp.PostNavigation?.Postt + " " + emp.PostNavigation?.PostCh,
                department = emp.DepartmentNavigation?.DepartmentName + " " + emp.DepartmentNavigation?.DepartmentCh,
                section = emp.SectionNavigation?.SectionName + " " + emp.SectionNavigation?.SectionCh,
                photoPath = webPhotoPath
            });
        }

        // Получение полной информации о сотруднике по ID в мод окне
        [HttpGet]
        public async Task<IActionResult> GetEmployeeFullDetails(int id)
        {
            var emp = await _context.Employees
                .Include(e => e.PostNavigation)
                .Include(e => e.DepartmentNavigation)
                .Include(e => e.SectionNavigation)
                .FirstOrDefaultAsync(e => e.CodeEmployee == id);

            if (emp == null)
                return NotFound();

            string tabnumStr = emp.TabNum?.ToString("D5");
            string photoFileName = $"{tabnumStr}.jpg";

            string rootPath = Directory.GetCurrentDirectory();
            string mainPath = Path.Combine(rootPath, "wwwroot", "Photo", photoFileName);
            string archivePath = Path.Combine(rootPath, "wwwroot", "Photo", "Archive", photoFileName);

            string webPhotoPath;
            if (System.IO.File.Exists(mainPath))
                webPhotoPath = Url.Content($"/Photo/{photoFileName}");
            else if (System.IO.File.Exists(archivePath))
                webPhotoPath = Url.Content($"/Photo/Archive/{photoFileName}");
            else
                webPhotoPath = Url.Content("~/images/default-profile.jpg");

            return Json(new
            {
                emp.CodeEmployee,
                emp.Surname,
                emp.Firstname,
                emp.Midname,
                emp.NameCh,
                emp.TabNum,
                emp.InputDate,
                emp.PartTime,
                emp.ContractNumber,
                emp.ContractDate,
                emp.Fired,
                emp.FiringDate,
                emp.Hazard,
                emp.Birthday,
                emp.Passport,
                emp.Address,

                Post = emp.PostNavigation != null ? emp.PostNavigation.Postt + " " + emp.PostNavigation.PostCh : null,
                Department = emp.DepartmentNavigation != null ? emp.DepartmentNavigation.DepartmentName + " " + emp.DepartmentNavigation.DepartmentCh : null,
                Section = emp.SectionNavigation != null ? emp.SectionNavigation.SectionName + " " + emp.SectionNavigation.SectionCh : null,
                Quota = emp.CodeQuotaNavigation?.Quotaa,

                HazardDoc = _context.CompanyDocs.FirstOrDefault(d => d.Code == emp.HazardDoc)?.Title,
                Sex = _context.Sexes.FirstOrDefault(s => s.CodeSex == emp.Sex)?.Sex,
                Citizenship = _context.Citizenships.FirstOrDefault(c => c.CodeCitizenship == emp.Citizenship)?.Citizenship,
                Nationality = _context.Nationalities.FirstOrDefault(n => n.CodeNationality == emp.Nationality)?.Nationality,
                District = _context.Districts.FirstOrDefault(d => d.CodeDistrict == emp.District)?.District,

                Photo = webPhotoPath
            });
        }



    }
}
