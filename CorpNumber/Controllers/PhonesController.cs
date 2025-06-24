using CorpNumber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class PhonesController : Controller
{
    private readonly CorpNumberDbContext _context;

    public PhonesController(CorpNumberDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? operatorId)
    {
        // Подгружаем операторов для фильтра
        ViewBag.Operators = await _context.Operators
            .OrderBy(o => o.Title)
            .ToListAsync();

        // Загружаем телефоны с навигацией
        var query = _context.Phones
            .Include(p => p.OperatorNavigation)
            .Include(p => p.TariffNavigation)
            .Include(p => p.StatusNavigation)
            .Include(p => p.InternetNavigation)
            .Include(p => p.AccountNavigation)
            .AsQueryable();

        // Фильтрация по оператору
        if (operatorId.HasValue && operatorId != 0)
        {
            query = query.Where(p => p.Operator == operatorId.Value);
        }

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

        return View(phoneViewModels);
    }
}
