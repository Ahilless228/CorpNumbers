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

    public async Task<IActionResult> Index(int? operatorId, int? categoryId, bool? onlyCorp)
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

        return View(phoneViewModels);
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
        string tabNum = emp?.TabNum?.ToString() ?? "—";

        return Json(new
        {
            category,
            organization,
            employee,
            tabNum,
            photoUrl = photoPath ?? Url.Content("~/images/default-profile.jpg")
        });
    }



}
