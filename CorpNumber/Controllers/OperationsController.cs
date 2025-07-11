using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorpNumber.Models;

namespace CorpNumber.Controllers
{
    public class OperationsController : Controller
    {
        private readonly CorpNumberDbContext _context;
        private const int PageSize = 100;

        public OperationsController(CorpNumberDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OperationsIndex(string? searchNumber, DateTime? dateFrom, DateTime? dateTo, int page = 1, int pageSize = 100)
        {
            var query = _context.Operations
                .Include(o => o.OperationTypes)
                .Include(o => o.Phone)
                .AsQueryable();

            // 🔍 Фильтрация по номеру
            if (!string.IsNullOrWhiteSpace(searchNumber))
            {
                query = query.Where(o => o.Phone != null &&
                                         o.Phone.Number != null &&
                                         o.Phone.Number.ToString().StartsWith(searchNumber));
            }

            // 📅 Фильтрация по датам
            if (dateFrom.HasValue)
            {
                query = query.Where(o => o.RequestDate >= dateFrom.Value.Date);
            }
            if (dateTo.HasValue)
            {
                query = query.Where(o => o.RequestDate <= dateTo.Value.Date);
            }

            // 🔢 Подсчёт записей
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // ⏬ Получение данных
            var operations = await query
                .OrderByDescending(o => o.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OperationViewModel
                {
                    CodeOperation = o.CodeOperation,
                    CodeOperType = o.CodeOperType,
                    RequestDate = o.RequestDate,
                    OperDate = o.OperDate,
                    Number = o.Number,
                    Complete = o.Complete,
                    Comments = o.Comments,
                    Information = o.Information,
                    OrderN = o.OrderN,
                    Type = o.OperationTypes != null ? o.OperationTypes.Type : null,
                    PhoneNumber = o.Phone != null ? o.Phone.Number.ToString() : "—"
                })
                .ToListAsync();

            // 📦 Передаём параметры во ViewBag
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.StartItem = (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalItems);
            ViewBag.SearchNumber = searchNumber;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            // 🔁 Если это AJAX-запрос — возвращаем только таблицу
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_OperationsTable", operations);
            }

            // 🖥 Обычный запрос — возвращаем всю страницу
            return View(operations);
        }


        // 👉 Метод для фильтрации, пагинации, ajax-загрузки таблицы
        [HttpGet]
        public async Task<IActionResult> GetFilteredOperations(string? searchNumber, DateTime? dateFrom, DateTime? dateTo, int page = 1)
        {
            var query = _context.Operations
                .Include(o => o.OperationTypes)
                .Include(o => o.Phone)
                .AsQueryable();

            // Фильтрация по номеру
            if (!string.IsNullOrEmpty(searchNumber))
            {
                query = query.Where(o => o.Phone != null && o.Phone.Number.ToString().StartsWith(searchNumber));
            }

            // Фильтрация по датам
            if (dateFrom.HasValue)
            {
                query = query.Where(o => o.RequestDate >= dateFrom.Value.Date);
            }
            if (dateTo.HasValue)
            {
                query = query.Where(o => o.RequestDate <= dateTo.Value.Date);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var operations = await query
                .OrderByDescending(o => o.RequestDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(o => new OperationViewModel
                {
                    CodeOperation = o.CodeOperation,
                    CodeOperType = o.CodeOperType,
                    RequestDate = o.RequestDate,
                    OperDate = o.OperDate,
                    Number = o.Number,
                    Complete = o.Complete,
                    Comments = o.Comments,
                    Information = o.Information,
                    OrderN = o.OrderN,
                    Type = o.OperationTypes != null ? o.OperationTypes.Type : null,
                    PhoneNumber = o.Phone != null ? o.Phone.Number.ToString() : "—"
                })
                .ToListAsync();

            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.StartItem = (page - 1) * PageSize + 1;
            ViewBag.EndItem = Math.Min(page * PageSize, totalItems);
            ViewBag.SearchNumber = searchNumber;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_OperationsTable", operations);
            }

            return View("OperationsIndex", operations);
        }
    }
}
