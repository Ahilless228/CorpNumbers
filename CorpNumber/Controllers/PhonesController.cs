using CorpNumber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PhonesController : Controller
{
    private readonly CorpNumberDbContext _context;

    public PhonesController(CorpNumberDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        System.Diagnostics.Debug.WriteLine("Index method started");

        var phones = await _context.Phones
            .Include(p => p.OperatorNavigation)
            .Include(p => p.TariffNavigation)
            .Include(p => p.StatusNavigation)
            .Include(p => p.InternetNavigation)
            .Include(p => p.AccountNavigation)
            .ToListAsync();

        // Временный вывод в консоль или отладчик
        System.Diagnostics.Debug.WriteLine($"Phones count: {phones.Count}");

        var phoneViewModels = phones.Select(p => new PhoneViewModel
        {
            CodePhone = p.CodePhone,
            Number = p.Number?.ToString() ?? "—",
            ICCID = p.ICCID ?? "—",
            Operator = p.OperatorNavigation?.Title ?? "—",
            Account = p.AccountNavigation?.Type ?? "—",  // <- здесь отображай название
            Tariff = p.TariffNavigation?.Title ?? "—",
            Status = p.StatusNavigation?.StatusText ?? "—",
            Internet = p.InternetNavigation?.Service ?? "—",
            Limit = p.Limit,
            Corporative = p.Corporative ?? false
        }).ToList();



        System.Diagnostics.Debug.WriteLine($"PhoneViewModels count: {phoneViewModels.Count}");

        return View(phoneViewModels);
    }
}
