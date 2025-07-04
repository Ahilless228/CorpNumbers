using CorpNumber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class PhonesController : Controller
{
    private readonly CorpNumberDbContext _context;

    public PhonesController(CorpNumberDbContext context)
    {
        _context = context;
    }
    public IActionResult EditPanel()
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
            return RedirectToAction("Login", "Auth");

        // Логика отображения как в Index
        return RedirectToAction("Index"); // или свой View с админ-кнопками
    }
    // Метод для отображения списка телефонов

    public async Task<IActionResult> Index(int? operatorId, int? categoryId)
    {
        ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

        var allowedCategories = new[] { 1, 6 };

        var query = _context.Phones
            .Include(p => p.OperatorNavigation)
            .Include(p => p.CodeOwnerNavigation)
                .ThenInclude(o => o.EmployeeNavigation)
            .Include(p => p.CodeOwnerNavigation.CategoryNavigation)
            .Where(p => p.Corporative == true &&
                        p.Account == 1 &&
                        p.CodeOwnerNavigation != null &&
                        allowedCategories.Contains(p.CodeOwnerNavigation.CodeCategory ?? -1));

        if (operatorId.HasValue && operatorId.Value != 0)
            query = query.Where(p => p.Operator == operatorId.Value);

        if (categoryId.HasValue && categoryId.Value != 0)
            query = query.Where(p => p.CodeOwnerNavigation.CodeCategory == categoryId.Value);

        var phones = await query.ToListAsync();

        var phoneViewModels = phones.Select(p => new PhoneViewModel
        {
            CodePhone = p.CodePhone,
            Number = p.Number?.ToString() ?? "—",
            Operator = p.OperatorNavigation?.Title ?? "—",
            FullName = p.CodeOwnerNavigation?.EmployeeNavigation != null
                ? $"{p.CodeOwnerNavigation.EmployeeNavigation.Surname} {p.CodeOwnerNavigation.EmployeeNavigation.Firstname} {p.CodeOwnerNavigation.EmployeeNavigation.Midname}".Trim()
                : "—",
            NameCh = p.CodeOwnerNavigation?.EmployeeNavigation?.NameCh ?? "—"
        }).ToList();

        ViewBag.PhoneCount = phoneViewModels.Count;

        ViewBag.Operators = await _context.Operators.ToListAsync();
        ViewBag.Categories = await _context.OwnerCategories
            .Where(c => allowedCategories.Contains(c.CodeCategory))
            .ToListAsync();

        ViewBag.SelectedOperator = operatorId ?? 0;
        ViewBag.SelectedCategory = categoryId ?? 0;

        return View(phoneViewModels);
    }


    // Метод для экспорта в Excel
    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int? operatorId, int? categoryId)
    {
        var allowedCategories = new[] { 1, 6 };

        var query = _context.Phones
            .Include(p => p.OperatorNavigation)
            .Include(p => p.CodeOwnerNavigation)
                .ThenInclude(o => o.EmployeeNavigation)
            .Include(p => p.CodeOwnerNavigation.CategoryNavigation)
            .Where(p => p.Corporative == true &&
                        p.Account == 1 &&
                        p.CodeOwnerNavigation != null &&
                        allowedCategories.Contains(p.CodeOwnerNavigation.CodeCategory ?? -1));

        if (operatorId.HasValue && operatorId.Value != 0)
            query = query.Where(p => p.Operator == operatorId.Value);

        if (categoryId.HasValue && categoryId.Value != 0)
            query = query.Where(p => p.CodeOwnerNavigation.CodeCategory == categoryId.Value);

        var phones = await query.ToListAsync();

        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Телефоны");

        // Заголовки
        worksheet.Cells[1, 1].Value = "№";
        worksheet.Cells[1, 2].Value = "Номер";
        worksheet.Cells[1, 3].Value = "ФИО";
        worksheet.Cells[1, 4].Value = "姓名";
        worksheet.Cells[1, 5].Value = "Оператор";

        // Данные
        for (int i = 0; i < phones.Count; i++)
        {
            var emp = phones[i].CodeOwnerNavigation?.EmployeeNavigation;
            worksheet.Cells[i + 2, 1].Value = phones[i].CodePhone;
            worksheet.Cells[i + 2, 2].Value = phones[i].Number?.ToString() ?? "—";
            worksheet.Cells[i + 2, 3].Value = emp != null
                ? $"{emp.Surname} {emp.Firstname} {emp.Midname}".Trim()
                : "—";
            worksheet.Cells[i + 2, 4].Value = emp?.NameCh ?? "—";
            worksheet.Cells[i + 2, 5].Value = phones[i].OperatorNavigation?.Title ?? "—";
        }

        worksheet.Cells.AutoFitColumns();

        var excelData = package.GetAsByteArray();
        var fileName = $"Телефоны_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }




    // 👇 Вот сюда вставляем новый метод
    [HttpGet]
    public IActionResult GetDetails(int id)
    {
        var phone = _context.Phones
            .Include(p => p.CodeOwnerNavigation)
                .ThenInclude(o => o.CategoryNavigation)
            .Include(p => p.CodeOwnerNavigation.EmployeeNavigation)  // Включаем сотрудника через владельца
            .FirstOrDefault(p => p.CodePhone == id);

        if (phone == null)
            return NotFound();

        // Получаем сотрудника через владельца
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
            {
                departmentFull = (dep.DepartmentName ?? "") + " " + (dep.DepartmentCh ?? "");
            }

            var post = _context.Posts.FirstOrDefault(p => p.CodePost == emp.Post);
            if (post != null)
            {
                postFull = (post.Postt ?? "") + " " + (post.PostCh ?? "");
            }
        }


        string photoFileName = emp?.TabNum?.ToString("D5"); // форматирует число в 5-значное с ведущими нулями

        string photoPath = null;

        if (!string.IsNullOrEmpty(photoFileName))
        {
            string mainPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", $"{photoFileName}.jpg");
            string archivePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", "Archive", $"{photoFileName}.jpg");

            if (System.IO.File.Exists(mainPath))
            {
                photoPath = Url.Content($"/Photo/{photoFileName}.jpg");
            }
            else if (System.IO.File.Exists(archivePath))
            {
                photoPath = Url.Content($"/Photo/Archive/{photoFileName}.jpg");
            }
        }
        if (string.IsNullOrEmpty(photoFileName))
        {
            var categoryCode = phone.CodeOwnerNavigation?.CodeCategory ?? -1;
            switch (categoryCode)
            {
                case 7: // Стационарный
                    photoPath = Url.Content("~/images/stat.jpg");
                    break;
                case 8: // Силовые структуры
                    photoPath = Url.Content("~/images/logomvd1.jpg");
                    break;
                case 4: // Резерв
                    photoPath = Url.Content("~/images/simcard.jpg");
                    break;
                default:
                    photoPath = Url.Content("~/images/default-profile.jpg");
                    break;
            }
        }

        string tabNum = emp?.TabNum?.ToString() ?? "—";

        return Json(new
        {
            category,
            organization,
            employee,
            tabNum,
            photoUrl = photoPath ?? Url.Content("~/images/default-profile.jpg"),
            department = departmentFull,
            post = postFull
        });
    }



}
