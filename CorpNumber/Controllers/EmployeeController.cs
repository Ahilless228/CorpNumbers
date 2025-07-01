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
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.CodeEmployee == id);
            if (emp == null) return NotFound();

            // Фото
            string photoPath;
            var fileName = emp.TabNum?.ToString("D5") + ".jpg";
            var pathMain = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", fileName);
            var pathArch = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", "Archive", fileName);

            if (System.IO.File.Exists(pathMain))
                photoPath = Url.Content($"/Photo/{fileName}");
            else if (System.IO.File.Exists(pathArch))
                photoPath = Url.Content($"/Photo/Archive/{fileName}");
            else
                photoPath = Url.Content("~/images/default-profile.jpg");

            // PartTime список с первым элементом "—"
            var partTimeList = _context.Posts
                .Select(p => new NamedId
                {
                    Code = p.CodePost,
                    Title = p.Postt + " " + p.PostCh
                }).ToList();
            partTimeList.Insert(0, new NamedId { Code = null, Title = "—" });

            // Название совмещения (или "—")
            string partTimeTitle = emp.PartTime != null
                ? _context.Posts
                    .Where(p => p.CodePost == emp.PartTime)
                    .Select(p => p.Postt + " " + p.PostCh)
                    .FirstOrDefault()
                : "—";

            return Json(new
            {
                codeEmployee = emp.CodeEmployee,
                surname = emp.Surname,
                firstname = emp.Firstname,
                midname = emp.Midname,
                nameCh = emp.NameCh,
                tabNum = emp.TabNum?.ToString("D5"),
                inputDate = emp.InputDate,
                contractNumber = emp.ContractNumber,
                contractDate = emp.ContractDate,
                fired = emp.Fired ?? false,
                firingDate = emp.FiringDate,
                hazard = emp.Hazard,
                hazardDoc = emp.HazardDoc,
                sex = emp.Sex,
                birthday = emp.Birthday,
                passport = emp.Passport,
                citizenship = emp.Citizenship,
                nationality = emp.Nationality,
                address = emp.Address,
                district = emp.District,
                post = emp.Post,
                partTime = emp.PartTime,
                partTimeTitle = partTimeTitle,
                partTimeList = partTimeList,
                department = emp.Department,
                section = emp.Section,
                quota = emp.CodeQuota,
                photo = photoPath,

                posts = _context.Posts.Select(p => new NamedId
                {
                    Code = p.CodePost,
                    Title = p.Postt + " " + p.PostCh
                }).ToList(),

                departments = _context.Departments.Select(d => new NamedId
                {
                    Code = d.CodeDepartment,
                    Title = d.DepartmentName + " " + d.DepartmentCh
                }).ToList(),

                sections = _context.Sections.Select(s => new NamedId
                {
                    Code = s.CodeSection,
                    Title = s.SectionName + " " + s.SectionCh
                }).ToList(),

                quotas = _context.Quotas.Select(q => new NamedId
                {
                    Code = q.CodeQuota,
                    Title = q.Quotaa != null ? q.Quotaa.ToString() : "—"
                }).ToList(),


                hazardDocs = _context.CompanyDocs.Select(h => new NamedId
                {
                    Code = h.Code,
                    Title = h.Title
                }).ToList(),

                sexes = _context.Sexes.Select(s => new NamedId
                {
                    Code = s.CodeSex,
                    Title = s.Sex + " " + s.SexCh
                }).ToList(),

                citizenships = _context.Citizenships.Select(c => new NamedId
                {
                    Code = c.CodeCitizenship,
                    Title = c.Citizenship + " " + c.CitizenshipCh
                }).ToList(),

                nationalities = _context.Nationalities.Select(n => new NamedId
                {
                    Code = n.CodeNationality,
                    Title = n.Nationality + " " + n.NationalityCh
                }).ToList(),

                districts = _context.Districts.Select(d => new NamedId
                {
                    Code = d.CodeDistrict,
                    Title = d.District + " " + d.DistrictCh
                }).ToList()
            });
        }

        public class NamedId
        {
            public int? Code { get; set; }
            public string?Title { get; set; }
        }





    }
}
