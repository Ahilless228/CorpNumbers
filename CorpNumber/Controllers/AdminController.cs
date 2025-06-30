using CorpNumber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace CorpNumber.Controllers
{
    public class AdminController : Controller
    {
        private readonly CorpNumberDbContext _context;

        public AdminController(CorpNumberDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminIndex(int? operatorId, int? categoryId, bool? onlyCorp)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login", "Auth");

            var query = _context.Phones
                .Include(p => p.OperatorNavigation)
                .Include(p => p.TariffNavigation)
                .Include(p => p.StatusNavigation)
                .Include(p => p.InternetNavigation)
                .Include(p => p.AccountNavigation)
                .Include(p => p.CodeOwnerNavigation)
                    .ThenInclude(o => o.CategoryNavigation)
                .AsQueryable();

            if (operatorId.HasValue && operatorId.Value != 0)
                query = query.Where(p => p.Operator == operatorId.Value);

            if (categoryId.HasValue && categoryId.Value != 0)
                query = query.Where(p => p.CodeOwnerNavigation != null &&
                                         p.CodeOwnerNavigation.CodeCategory == categoryId.Value);

            if (onlyCorp.HasValue && onlyCorp.Value)
                query = query.Where(p => p.Corporative == true);

            var phones = await query.ToListAsync();

            var phoneViewModels = phones.Select(p => new PhoneViewModel
            {
                CodePhone = p.CodePhone,
                Number = p.Number?.ToString() ?? "—",
                ICCID = p.ICCID ?? "—",
                Operator = p.OperatorNavigation?.Title ?? "—",
                Account = p.AccountNavigation?.Type ?? "—",
                Tariff = p.TariffNavigation?.Title ?? "—",
                Status = p.StatusNavigation?.StatusText ?? "—",
                Internet = p.InternetNavigation?.Service ?? "—",
                Limit = p.Limit,
                Corporative = p.Corporative ?? false
            }).ToList();

            ViewBag.PhoneCount = phoneViewModels.Count;
            ViewBag.Operators = await _context.Operators.ToListAsync();
            ViewBag.Categories = await _context.OwnerCategories.ToListAsync();
            ViewBag.SelectedOperator = operatorId ?? 0;
            ViewBag.SelectedCategory = categoryId ?? 0;
            ViewBag.OnlyCorp = onlyCorp ?? false;
            //кнопки с счетчиками
            ViewBag.IncompleteOperations = await _context.Operations.CountAsync(op => op.Complete == false);
            ViewBag.SimCardDeliveries = await _context.SimCards.CountAsync(sc =>
                sc.Incomplete == false && sc.IssueDate == null && sc.Operator == 1);


            return View("AdminIndex", phoneViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? operatorId, int? categoryId, bool? onlyCorp)
        {
            var query = _context.Phones
                .Include(p => p.OperatorNavigation)
                .Include(p => p.TariffNavigation)
                .Include(p => p.StatusNavigation)
                .Include(p => p.InternetNavigation)
                .Include(p => p.AccountNavigation)
                .Include(p => p.CodeOwnerNavigation)
                    .ThenInclude(o => o.CategoryNavigation)
                .AsQueryable();

            if (operatorId.HasValue && operatorId.Value != 0)
                query = query.Where(p => p.Operator == operatorId.Value);

            if (categoryId.HasValue && categoryId.Value != 0)
                query = query.Where(p => p.CodeOwnerNavigation != null &&
                                         p.CodeOwnerNavigation.CodeCategory == categoryId.Value);

            if (onlyCorp.HasValue && onlyCorp.Value)
                query = query.Where(p => p.Corporative == true);

            var phones = await query.ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Телефоны");

            worksheet.Cells[1, 1].Value = "№";
            worksheet.Cells[1, 2].Value = "Номер";
            worksheet.Cells[1, 3].Value = "ICCID";
            worksheet.Cells[1, 4].Value = "Оператор";
            worksheet.Cells[1, 5].Value = "Счёт";
            worksheet.Cells[1, 6].Value = "Тарифный план";
            worksheet.Cells[1, 7].Value = "Состояние";
            worksheet.Cells[1, 8].Value = "Интернет";
            worksheet.Cells[1, 9].Value = "Лимит";
            worksheet.Cells[1, 10].Value = "Корпоративный";

            for (int i = 0; i < phones.Count; i++)
            {
                var p = phones[i];
                worksheet.Cells[i + 2, 1].Value = p.CodePhone;
                worksheet.Cells[i + 2, 2].Value = p.Number;
                worksheet.Cells[i + 2, 3].Value = p.ICCID;
                worksheet.Cells[i + 2, 4].Value = p.OperatorNavigation?.Title;
                worksheet.Cells[i + 2, 5].Value = p.AccountNavigation?.Type;
                worksheet.Cells[i + 2, 6].Value = p.TariffNavigation?.Title;
                worksheet.Cells[i + 2, 7].Value = p.StatusNavigation?.StatusText;
                worksheet.Cells[i + 2, 8].Value = p.InternetNavigation?.Service;
                worksheet.Cells[i + 2, 9].Value = p.Limit?.ToString() ?? "—";
                worksheet.Cells[i + 2, 10].Value = p.Corporative == true ? "Да" : "Нет";
            }

            worksheet.Cells.AutoFitColumns();

            var excelData = package.GetAsByteArray();
            var fileName = $"Телефоны_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public IActionResult GetDetails(int id)
        {
            var phone = _context.Phones
                .Include(p => p.CodeOwnerNavigation)
                    .ThenInclude(o => o.CategoryNavigation)
                .Include(p => p.CodeOwnerNavigation.EmployeeNavigation)
                .FirstOrDefault(p => p.CodePhone == id);

            if (phone == null)
                return NotFound();

            var emp = phone.CodeOwnerNavigation?.EmployeeNavigation;

            string category = phone.CodeOwnerNavigation?.CategoryNavigation?.Category ?? "—";
            string organization = "—";

            if (phone.CodeOwner == null)
                organization = "";
            else if (new[] { 1, 4, 6, 7 }.Contains(phone.CodeOwnerNavigation.CodeCategory ?? -1))
                organization = "ОсОО \"Алтынкен\"";
            else if (new[] { 2, 8, 10 }.Contains(phone.CodeOwnerNavigation.CodeCategory ?? -1))
                organization = _context.OtherOwners
                    .Where(o => o.CodeOthers == phone.CodeOwnerNavigation.CodeOthers)
                    .Select(o => o.Title)
                    .FirstOrDefault() ?? "—";

            string employee = emp != null
                ? $"{emp.Surname} {emp.Firstname} {emp.Midname} {emp.NameCh}".Trim()
                : "—";

            string departmentFull = "—";
            string postFull = "—";

            if (emp != null && new[] { 1, 6 }.Contains(phone.CodeOwnerNavigation.CodeCategory ?? -1))
            {
                var dep = _context.Departments.FirstOrDefault(d => d.CodeDepartment == emp.Department);
                if (dep != null)
                    departmentFull = (dep.DepartmentName ?? "") + " " + (dep.DepartmentCh ?? "");

                var post = _context.Posts.FirstOrDefault(p => p.CodePost == emp.Post);
                if (post != null)
                    postFull = (post.Postt ?? "") + " " + (post.PostCh ?? "");
            }

            string photoFileName = emp?.TabNum?.ToString("D5");
            string photoPath = null;

            if (!string.IsNullOrEmpty(photoFileName))
            {
                string mainPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", $"{photoFileName}.jpg");
                string archivePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", "Archive", $"{photoFileName}.jpg");

                if (System.IO.File.Exists(mainPath))
                    photoPath = Url.Content($"/Photo/{photoFileName}.jpg");
                else if (System.IO.File.Exists(archivePath))
                    photoPath = Url.Content($"/Photo/Archive/{photoFileName}.jpg");
            }

            if (string.IsNullOrEmpty(photoPath))
            {
                var categoryCode = phone.CodeOwnerNavigation?.CodeCategory ?? -1;
                photoPath = categoryCode switch
                {
                    7 => Url.Content("~/images/stat.jpg"),
                    8 => Url.Content("~/images/logomvd1.jpg"),
                    4 => Url.Content("~/images/simcard.jpg"),
                    _ => Url.Content("~/images/default-profile.jpg")
                };
            }

            string tabNum = emp?.TabNum?.ToString() ?? "—";

            return Json(new
            {
                category,
                organization,
                employee,
                tabNum,
                photoUrl = photoPath,
                department = departmentFull,
                post = postFull
            });
     
        }
        public IActionResult Exit()
        {
            HttpContext.Session.Remove("IsAdmin"); // или .Clear()
            return RedirectToAction("Index", "Phones");
        }
        // Загрузка данных модального окна для получения информации о сотруднике по ID телефона
        [HttpGet]
        public IActionResult GetEmployeeDetails(int id)
        {
            var phone = _context.Phones
                .Include(p => p.CodeOwnerNavigation)
                    .ThenInclude(o => o.EmployeeNavigation)
                .FirstOrDefault(p => p.CodePhone == id);

            var emp = phone?.CodeOwnerNavigation?.EmployeeNavigation;

            if (emp == null)
                return Json(null);

            var department = _context.Departments.FirstOrDefault(d => d.CodeDepartment == emp.Department);
            var section = _context.Sections.FirstOrDefault(s => s.CodeSection == emp.Section);
            var post = _context.Posts.FirstOrDefault(p => p.CodePost == emp.Post);
            var quotaText = _context.Quotas
              .Where(q => q.CodeQuota == emp.CodeQuota)
              .Select(q => (q.Quotaa != null ? q.Quotaa.ToString() : "—"))
              .FirstOrDefault();



            string photoFileName = emp.TabNum?.ToString("D5");
            string photoPath = null;

            if (!string.IsNullOrEmpty(photoFileName))
            {
                string mainPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", $"{photoFileName}.jpg");
                string archivePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", "Archive", $"{photoFileName}.jpg");

                if (System.IO.File.Exists(mainPath))
                    photoPath = Url.Content($"/Photo/{photoFileName}.jpg");
                else if (System.IO.File.Exists(archivePath))
                    photoPath = Url.Content($"/Photo/Archive/{photoFileName}.jpg");
            }

            if (string.IsNullOrEmpty(photoPath))
                photoPath = Url.Content("~/images/default-profile.jpg");



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
                Quota = quotaText,
                Post = post?.Postt + " " + post?.PostCh,
                Department = department?.DepartmentName + " " + department?.DepartmentCh,
                Section = section?.SectionName,
                Photo = photoPath,
                Org = "ОсОО \"Алтынкен\"",
                Hazard = emp.Hazard,
                ContractNumber = emp.ContractNumber,
                ContractDate = emp.ContractDate,
                Fired = emp.Fired,
                FiringDate = emp.FiringDate,
                HazardDocTitle = _context.CompanyDocs
                .Where(cd => cd.Code == emp.HazardDoc)
                .Select(cd => cd.Title)
                .FirstOrDefault(),
                SexTitle = _context.Sexes
                    .Where(s => s.CodeSex == emp.Sex)
                    .Select(s => s.Sex + " " + s.SexCh)
                    .FirstOrDefault(),

                CitizenshipTitle = _context.Citizenships
                    .Where(c => c.CodeCitizenship == emp.Citizenship)
                    .Select(c => c.Citizenship + " " + c.CitizenshipCh)
                    .FirstOrDefault(),

                NationalityTitle = _context.Nationalities
                    .Where(n => n.CodeNationality == emp.Nationality)
                    .Select(n => n.Nationality + " " + n.NationalityCh)
                    .FirstOrDefault(),

                Birthday = emp.Birthday,
                Passport = emp.Passport,
                Address = emp.Address,
                DistrictTitle = _context.Districts
                    .Where(d => d.CodeDistrict == emp.District)
                    .Select(d => d.District + " " + d.DistrictCh)
                    .FirstOrDefault(),



            });
        }

        [HttpGet]
        public IActionResult GetRegisteredPhones(int id)
        {
            // Ищем всех владельцев, связанных с этим сотрудником
            var owners = _context.Owners
                .Where(o => o.CodeEmployee == id)
                .Select(o => o.CodeOwner)
                .ToList();

            if (!owners.Any())
                return Json(new { count = 0, phones = new List<object>() });

            // Ищем все номера, у которых CodeOwner совпадает с владельцем
            var phones = _context.Phones
                .Where(p => p.CodeOwner != null && owners.Contains(p.CodeOwner.Value))
                .Include(p => p.OperatorNavigation)
                .Include(p => p.AccountNavigation)
                .ToList();

            var result = phones.Select(phone =>
            {
                // Находим последнюю операцию "выдача номера" (CodeOperType == 6)
                var issueDate = _context.Operations
                    .Where(op => op.Number == phone.CodePhone &&
                                 op.CodeOperType == 6 &&
                                 op.Owner_new != null &&
                                 owners.Contains(op.Owner_new.Value))
                    .OrderByDescending(op => op.OperDate)
                    .Select(op => op.OperDate)
                    .FirstOrDefault();

                return new
                {
                    number = phone.Number?.ToString() ?? "—",
                    operatorTitle = phone.OperatorNavigation?.Title ?? "—",
                    accountType = phone.AccountNavigation?.Type ?? "—",
                    issueDate = issueDate?.ToString("yyyy-MM-dd") ?? "—",
                    corporative = phone.Corporative == true
                };
            }).ToList();

            return Json(new
            {
                count = result.Count,
                phones = result
            });
        }



    }
}
