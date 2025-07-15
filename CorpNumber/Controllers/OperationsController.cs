using Azure;
using CorpNumber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // 👉 Метод для получения информации об операции по ID
        [HttpGet]
        public async Task<IActionResult> GetOperationInfo(int id)
        {
            var operation = await _context.Operations
                .Include(o => o.OperationTypes)
                .Include(o => o.Phone)
                    .ThenInclude(p => p.OperatorNavigation) // предполагается навигация
                .Include(o => o.Phone)
                    .ThenInclude(p => p.AccountNavigation) // предполагается навигация
                .Include(o => o.OwnerOld)
                    .ThenInclude(owner => owner.EmployeeNavigation)
                .Include(o => o.OwnerOld)
                    .ThenInclude(owner => owner.CategoryNavigation)
                .Include(o => o.OwnerNew)
                    .ThenInclude(owner => owner.EmployeeNavigation)
                .Include(o => o.OwnerNew)
                    .ThenInclude(owner => owner.CategoryNavigation)
                .FirstOrDefaultAsync(o => o.CodeOperation == id);
                


            if (operation == null)
                return NotFound();

            string oldValue = "—", newValue = "—";

            switch (operation.CodeOperType)
            {
                case 1:
                case 2:
                    oldValue = await _context.Statuses
                        .Where(s => s.Code == operation.Status_old)
                        .Select(s => s.StatusText)
                        .FirstOrDefaultAsync() ?? "—";
                    newValue = await _context.Statuses
                        .Where(s => s.Code == operation.Status_new)
                        .Select(s => s.StatusText)
                        .FirstOrDefaultAsync() ?? "—";
                    break;


                case 3:
                    oldValue = operation.ICCID_old ?? "—";
                    newValue = operation.ICCID_new ?? "—";
                    break;

                case 4:
                case 5:
                    oldValue = await _context.InternetServices
                        .Where(s => s.CodeServ == operation.Internet_old)
                        .Select(s => s.Service)
                        .FirstOrDefaultAsync() ?? "—";
                    newValue = await _context.InternetServices
                        .Where(s => s.CodeServ == operation.Internet_new)
                        .Select(s => s.Service)
                        .FirstOrDefaultAsync() ?? "—";
                    break;

                case 6:
                case 7:
                    {
                        string FormatEmployeeInfo(Employee employee)
                        {
                            var dep = _context.Departments.FirstOrDefault(d => d.CodeDepartment == employee.Department);
                            return $"{employee.Surname} {employee.Firstname} {employee.Midname} {employee.NameCh} ({dep?.DepartmentName} {dep?.DepartmentCh})";
                        }

                        string FormatTempOwnerInfo(TempOwners temp)
                        {
                            var dep = _context.Departments.FirstOrDefault(d => d.CodeDepartment == temp.HostDepartment);
                            return $"Временный пользователь: {temp.NameTO} {temp.NameTOCh}. Организация: {temp.Organization}. Принимающее управление: {dep?.DepartmentName} {dep?.DepartmentCh}.";
                        }

                        async Task<string> ResolveOwnerInfo(int? ownerId)
                        {
                            if (ownerId == null) return "—";

                            var owner = await _context.Owners
                                .Include(o => o.CategoryNavigation)
                                .Include(o => o.EmployeeNavigation)
                                .FirstOrDefaultAsync(o => o.CodeOwner == ownerId);

                            if (owner == null || owner.CodeCategory == null)
                                return "—";

                            switch (owner.CodeCategory)
                            {
                                case 4:
                                    return "Резерв";
                                case 1:
                                case 6:
                                    if (owner.EmployeeNavigation != null)
                                        return FormatEmployeeInfo(owner.EmployeeNavigation);
                                    break;
                                case 3:
                                    var temp = await _context.TempOwners.FirstOrDefaultAsync(t => t.CodeTempOwner == owner.CodeTempOwner);
                                    if (temp != null)
                                        return FormatTempOwnerInfo(temp);
                                    break;
                            }

                            return "—";
                        }

                        oldValue = await ResolveOwnerInfo(operation.Owner_old);
                        newValue = await ResolveOwnerInfo(operation.Owner_new);
                        break;
                    }


                case 8:
                    oldValue = operation.Limit_old?.ToString() ?? "—";
                    newValue = operation.Limit_new?.ToString() ?? "—";
                    break;

                case 9:
                case 10:
                    oldValue = await _context.Accounts
                        .Where(a => a.Code == operation.Account_old)
                        .Select(a => a.Type)
                        .FirstOrDefaultAsync() ?? "—";
                    newValue = await _context.Accounts
                        .Where(a => a.Code == operation.Account_new)
                        .Select(a => a.Type)
                        .FirstOrDefaultAsync() ?? "—";
                    break;

                case 11:
                    oldValue = "Вне корпоратива";
                    newValue = "В корпоративной группе. Счёт: " +
                        (await _context.Accounts
                            .Where(a => a.Code == operation.Account_new)
                            .Select(a => a.Type)
                            .FirstOrDefaultAsync() ?? "—");
                    break;

                case 12:
                    oldValue = "В корпоративной группе. Счёт: " +
                        (await _context.Accounts
                            .Where(a => a.Code == operation.Phone.Account)
                            .Select(a => a.Type)
                            .FirstOrDefaultAsync() ?? "—");
                    newValue = "Вне корпоратива";
                    break;

                case 16:
                    oldValue = await _context.Tariffs
                        .Where(t => t.CodeTariff == operation.Tariff_old)
                        .Select(t => t.Title)
                        .FirstOrDefaultAsync() ?? "—";
                    newValue = await _context.Tariffs
                        .Where(t => t.CodeTariff == operation.Tariff_new)
                        .Select(t => t.Title)
                        .FirstOrDefaultAsync() ?? "—";
                    break;

                default:
                    oldValue =  "—";
                    newValue = "—";
                    break;
            }

            var operatorTitle = await _context.Operators
                .Where(o => o.CodeOperator == operation.Phone.Operator)
                .Select(o => o.Title)
                .FirstOrDefaultAsync();

            var accountType = await _context.Accounts
                .Where(a => a.Code == operation.Phone.Account)
                .Select(a => a.Type)
                .FirstOrDefaultAsync();

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
                oldValue,
                newValue,
                complete = operation.Complete,
                orderN = operation.OrderN
            };

            return Json(result);
        }

        //дополнительный метод для получения владельца для кейсов 6 и 7
        private async Task<string> DescribeOwner(Owner? owner)
        {
            if (owner == null || owner.CodeCategory == null)
                return "—";

            switch (owner.CodeCategory)
            {
                case 4:
                    return "Резерв";

                case 1:
                case 6:
                    if (owner.EmployeeNavigation == null)
                    {
                        var emp = await _context.Employees
                            .FirstOrDefaultAsync(e => e.CodeEmployee == owner.CodeEmployee);

                        if (emp == null) return "—";

                        var dep = await _context.Departments
                            .FirstOrDefaultAsync(d => d.CodeDepartment == emp.Department);

                        return $"{emp.Surname} {emp.Firstname} {emp.Midname} {emp.NameCh} " +
                               $"({dep?.DepartmentName} {dep?.DepartmentCh})";
                    }
                    else
                    {
                        var emp = owner.EmployeeNavigation;
                        var dep = await _context.Departments
                            .FirstOrDefaultAsync(d => d.CodeDepartment == emp.Department);

                        return $"{emp.Surname} {emp.Firstname} {emp.Midname} {emp.NameCh} " +
                               $"({dep?.DepartmentName} {dep?.DepartmentCh})";
                    }

                case 3:
                    var temp = await _context.TempOwners
                        .FirstOrDefaultAsync(t => t.CodeTempOwner == owner.CodeTempOwner);

                    if (temp == null) return "Временный пользователь";

                    var hostDep = await _context.Departments
                        .FirstOrDefaultAsync(d => d.CodeDepartment == temp.HostDepartment);

                    return $"Временный пользователь: {temp.NameTO} {temp.NameTOCh}. " +
                           $"Организация: {temp.Organization}. " +
                           $"Принимающее управление: {hostDep?.DepartmentName} {hostDep?.DepartmentCh}.";

                default:
                    return "—";
            }
        }







        [HttpGet]
        public async Task<IActionResult> GetOperationEditData(int id)
        {
            var operation = await _context.Operations
                .Include(o => o.Phone)
                .Include(o => o.OperationTypes)
                .FirstOrDefaultAsync(o => o.CodeOperation == id);

            if (operation == null) return NotFound();

            string oldValue = "—", newValue = "—";

            switch (operation.CodeOperType)
            {
                case 1:
                case 2:
                    oldValue = operation.Status_old?.ToString() ?? "";
                    newValue = operation.Status_new?.ToString() ?? "";
                    break;

                case 3:
                    oldValue = operation.ICCID_old ?? "";
                    newValue = operation.ICCID_new ?? "";
                    break;

                case 4:
                case 5:
                    oldValue = operation.Internet_old?.ToString() ?? "";
                    newValue = operation.Internet_new?.ToString() ?? "";
                    break;

                case 6:
                case 7:
                    oldValue = operation.Owner_old?.ToString() ?? "";
                    newValue = operation.Owner_new?.ToString() ?? "";
                    break;


                case 8:
                    oldValue = operation.Limit_old?.ToString() ?? "";
                    newValue = operation.Limit_new?.ToString() ?? "";
                    break;

                case 9:
                case 10:
                    oldValue = operation.Account_old?.ToString() ?? "";
                    newValue = operation.Account_new?.ToString() ?? "";
                    break;

                case 11:
                    oldValue = "Вне корпоратива";
                    newValue = "В корпоративной группе. Счёт: " +
                        (await _context.Accounts
                            .Where(a => a.Code == operation.Account_new)
                            .Select(a => a.Type)
                            .FirstOrDefaultAsync() ?? "—");
                    break;

                case 12:
                    oldValue = "В корпоративной группе. Счёт: " +
                        (await _context.Accounts
                            .Where(a => a.Code == operation.Phone.Account)
                            .Select(a => a.Type)
                            .FirstOrDefaultAsync() ?? "—");
                    newValue = "Вне корпоратива";
                    break;

                case 16:
                    oldValue = operation.Tariff_old?.ToString() ?? "";
                    newValue = operation.Tariff_new?.ToString() ?? "";
                    break;


                default:
                    oldValue = "—";
                    newValue = "—";
                    break;
            }

            var result = new
            {
                requestDate = operation.RequestDate?.ToString("yyyy-MM-dd"),
                operDate = operation.OperDate?.ToString("yyyy-MM-dd"),
                information = operation.Information,
                comments = operation.Comments,
                codeOperType = operation.CodeOperType,
                phoneNumber = operation.Phone?.Number.ToString(),
                operatorCode = operation.Phone?.Operator,
                accountCode = operation.Phone?.Account,
                complete = operation.Complete,
                orderN = operation.OrderN,
                oldValue,
                newValue
            };

            return Json(result);
        }

        private async Task<string> DescribeOwnerById(int? ownerId)
        {
            if (ownerId == null) return "—";

            var owner = await _context.Owners
                .Include(o => o.CategoryNavigation)
                .Include(o => o.EmployeeNavigation)
                .FirstOrDefaultAsync(o => o.CodeOwner == ownerId);

            if (owner == null || owner.CodeCategory == null) return "—";

            switch (owner.CodeCategory)
            {
                case 4:
                    return "Резерв";

                case 1:
                case 6:
                    var emp = owner.EmployeeNavigation;
                    if (emp == null)
                    {
                        emp = await _context.Employees.FirstOrDefaultAsync(e => e.CodeEmployee == owner.CodeEmployee);
                    }

                    var dep = emp != null
                        ? await _context.Departments.FirstOrDefaultAsync(d => d.CodeDepartment == emp.Department)
                        : null;

                    return emp == null ? "—" : $"{emp.Surname} {emp.Firstname} {emp.Midname} {emp.NameCh} ({dep?.DepartmentName} {dep?.DepartmentCh})";

                case 3:
                    var temp = await _context.TempOwners.FirstOrDefaultAsync(t => t.CodeTempOwner == owner.CodeTempOwner);
                    if (temp == null) return "Временный пользователь";

                    var hostDep = await _context.Departments.FirstOrDefaultAsync(d => d.CodeDepartment == temp.HostDepartment);
                    return $"Временный пользователь: {temp.NameTO} {temp.NameTOCh}. Организация: {temp.Organization}. Принимающее управление: {hostDep?.DepartmentName} {hostDep?.DepartmentCh}.";

                default:
                    return "—";
            }
        }


        //обновление таблицы после сохранения изменений
        [HttpPost]
        public async Task<IActionResult> UpdateOperation(Operations model)
        {
            var operation = await _context.Operations
                .Include(o => o.Phone)
                .FirstOrDefaultAsync(o => o.CodeOperation == model.CodeOperation);

            if (operation == null)
                return NotFound();

            // Основные поля
            operation.RequestDate = model.RequestDate;
            operation.OperDate = model.OperDate;
            operation.Information = model.Information;
            operation.Comments = model.Comments;
            operation.CodeOperType = model.CodeOperType;
            operation.Complete = model.Complete;
            operation.OrderN = model.OrderN;

            // Справочные поля (если переданы)
            operation.Status_old = model.Status_old;
            operation.Status_new = model.Status_new;

            operation.Tariff_old = model.Tariff_old;
            operation.Tariff_new = model.Tariff_new;

            operation.Account_old = model.Account_old;
            operation.Account_new = model.Account_new;

            operation.ICCID_old = model.ICCID_old;
            operation.ICCID_new = model.ICCID_new;

            operation.Internet_old = model.Internet_old;
            operation.Internet_new = model.Internet_new;

            operation.Owner_old = model.Owner_old;
            operation.Owner_new = model.Owner_new;

            operation.Limit_old = model.Limit_old;
            operation.Limit_new = model.Limit_new;

            // Телефон — оператор и счёт
            if (operation.Phone != null && model.Phone != null)
            {
                operation.Phone.Operator = model.Phone.Operator;
                operation.Phone.Account = model.Phone.Account;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet]
        public IActionResult GetDropdownData()
        {
            var types = _context.OperationTypes.Select(t => new { t.CodeOperType, t.Type }).ToList();
            var accounts = _context.Accounts.Select(a => new { a.Code, a.Type }).ToList();
            var operators = _context.Operators.Select(o => new { o.CodeOperator, o.Title }).ToList();

            return Json(new { types, accounts, operators });
        }



        private string DescribeOwnerFast(Owner? owner)
        {
            if (owner == null || owner.CodeCategory == null)
                return "—";

            switch (owner.CodeCategory)
            {
                case 4:
                    return "Резерв";

                case 1:
                case 6:
                    var emp = owner.EmployeeNavigation;
                    if (emp == null) return "—";

                    var dep = emp.DepartmentNavigation;
                    string depText = dep != null
                        ? $"{dep.DepartmentName} {dep.DepartmentCh}".Trim()
                        : "";

                    return $"{emp.Surname} {emp.Firstname} {emp.Midname} {emp.NameCh}".Trim() +
                           (string.IsNullOrWhiteSpace(depText) ? "" : $" ({depText})");

                case 3:
                    var temp = owner.TempOwnerNavigation;
                    if (temp == null) return "Временный пользователь";

                    var hostDep = temp.DepartmentNavigation;
                    string hostDepText = hostDep != null
                        ? $"{hostDep.DepartmentName} {hostDep.DepartmentCh}".Trim()
                        : "";

                    return $"Временный пользователь: {temp.NameTO} {temp.NameTOCh}. " +
                           $"Организация: {temp.Organization}. " +
                           (string.IsNullOrWhiteSpace(hostDepText) ? "" : $"Принимающее управление: {hostDepText}.");

                default:
                    return "—";
            }
        }






        //-------------------------------------------Эндпоинты для получения данных для выпадающих списков-------------------------------------------
        [HttpGet]
        public IActionResult GetInternetServices()
        {
            var list = _context.InternetServices.Select(s => new { s.CodeServ, s.Service }).ToList();
            return Json(list);
        }

        [HttpGet]
        public IActionResult GetAccountOptions()
        {
            var list = _context.Accounts.Select(a => new { a.Code, a.Type }).ToList();
            return Json(list);
        }

        [HttpGet]
        public IActionResult GetTariffOptions()
        {
            var list = _context.Tariffs.Select(t => new { t.CodeTariff, t.Title }).ToList();
            return Json(list);
        }
        //метод отображения владельцев в выпадающем списке
        [HttpGet]
        public async Task<IActionResult> GetOwnerOptions()
        {
            var owners = await _context.Owners
                .Include(o => o.EmployeeNavigation)
                    .ThenInclude(e => e.DepartmentNavigation)
                .Include(o => o.TempOwnerNavigation)
                    .ThenInclude(t => t.DepartmentNavigation)
                .Include(o => o.CategoryNavigation)
                .ToListAsync();

            var results = new List<object>();

            foreach (var owner in owners)
            {
                string text = DescribeOwnerFast(owner);
                if (!string.IsNullOrWhiteSpace(text) && text != "—")
                {
                    results.Add(new
                    {
                        codeOwner = owner.CodeOwner,
                        display = text
                    });
                }
            }

            return Json(results);
        }



        [HttpGet]
        public IActionResult GetStatusOptions()
        {
            var statuses = _context.Statuses
                .Select(s => new { value = s.Code, text = s.StatusText })
                .ToList();

            return Json(statuses);
        }




    }
}
