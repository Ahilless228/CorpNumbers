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

        public async Task<IActionResult> OperationsIndex(string? searchNumber, string? orderNumber, string? operationType, DateTime? dateFrom, DateTime? dateTo, int page = 1, int pageSize = 100)
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
            if (!string.IsNullOrEmpty(orderNumber))
            {
                query = query.Where(o => o.OrderN != null && o.OrderN.ToString().Contains(orderNumber));
            }

            if (!string.IsNullOrEmpty(operationType))
            {
                query = query.Where(o => o.OperationTypes != null && o.OperationTypes.Type == operationType);
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
            ViewBag.OperationTypes = await _context.OperationTypes
                .Select(t => t.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

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
        public async Task<IActionResult> GetFilteredOperations(string? searchNumber, string? orderNumber, string? operationType, DateTime? dateFrom, DateTime? dateTo, int page = 1)
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
            if (!string.IsNullOrEmpty(orderNumber))
            {
                query = query.Where(o => o.OrderN != null && o.OrderN.ToString().Contains(orderNumber));
            }

            if (!string.IsNullOrEmpty(operationType))
            {
                query = query.Where(o => o.OperationTypes != null && o.OperationTypes.Type == operationType);
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
            ViewBag.OperationTypes = await _context.OperationTypes
                .Select(t => t.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();


            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_OperationsTable", operations);
            }

            return View("OperationsIndex", operations);
        }
        [HttpGet]
        public async Task<IActionResult> GetOperationInfo(int id)
        {
            var operation = await _context.Operations
                .Include(o => o.OperationTypes)
                .Include(o => o.Phone)
                    .ThenInclude(p => p.OperatorNavigation) // навигац. свойство к оператору
                .Include(o => o.Phone)
                    .ThenInclude(p => p.AccountNavigation) // навигац. свойство к счёту
                .FirstOrDefaultAsync(o => o.CodeOperation == id);


            if (operation == null)
            {
                return NotFound();
            }

            var result = new
            {
                phoneNumber = operation.Phone?.Number.ToString(),
                operatorName = operation.Phone?.OperatorNavigation?.Title, // ✅ не ключевое слово
                account = operation.Phone?.AccountNavigation?.Type,
                requestDate = operation.RequestDate?.ToString("yyyy-MM-dd"),
                operDate = operation.OperDate?.ToString("yyyy-MM-dd"),
                type = operation.OperationTypes?.Type,
                information = operation.Information,
                comments = operation.Comments,
                oldValue = operation.Number,
                newValue = operation.Complete?.ToString(),
                complete = operation.Complete,
                orderN = operation.OrderN
            };


            return Json(result);
        }


    }
}
