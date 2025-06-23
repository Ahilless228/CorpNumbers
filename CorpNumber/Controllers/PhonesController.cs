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
        var phones = await _context.Phones
            .Include(p => p.OperatorNavigation)
            .Include(p => p.TariffNavigation)
            .Include(p => p.StatusNavigation)
            .Include(p => p.InternetNavigation)
            .Select(p => new PhoneViewModel
            {
                CodePhone = p.CodePhone,
                Number = p.Number.ToString(),
                ICCID = p.ICCID,
                Operator = p.OperatorNavigation.Title,
                Account = p.Account.ToString(), // при наличии отдельной таблицы - сделай Include
                Tariff = p.TariffNavigation.Title,
                Status = p.StatusNavigation.StatusText,
                Internet = p.InternetNavigation.Service,
                Limit = p.Limit,
                Corporative = p.Corporative ?? false
            })
            .ToListAsync();

        return View(phones);
    }
}
