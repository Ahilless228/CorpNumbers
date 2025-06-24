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

    public async Task<IActionResult> Index(int? operatorId, int? categoryId)
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
        {
            int opId = operatorId.Value;
            query = query.Where(p => p.Operator == opId);
        }

        if (categoryId.HasValue && categoryId.Value != 0)
        {
            int catId = categoryId.Value;
            query = query.Where(p => p.CodeOwnerNavigation != null && p.CodeOwnerNavigation.CodeCategory == catId);
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

        ViewBag.Operators = await _context.Operators.ToListAsync();
        ViewBag.Categories = await _context.OwnerCategories.ToListAsync();
        ViewBag.SelectedOperator = operatorId ?? 0;
        ViewBag.SelectedCategory = categoryId ?? 0;

        return View(phoneViewModels);
    }
}
