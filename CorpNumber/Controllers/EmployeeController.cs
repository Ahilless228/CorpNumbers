using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorpNumber.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CorpNumber.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly CorpNumberDbContext _context;

        public EmployeeController(CorpNumberDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> EmployeeIndex(string search)
        {
            var query = _context.Employees
                .Include(e => e.PostNavigation)
                .Include(e => e.DepartmentNavigation)
                .Include(e => e.SectionNavigation)
                .Include(e => e.CodeQuotaNavigation)
                .Where(e => e.Fired != true)
                .AsQueryable();


            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    (e.Surname + " " + e.Firstname + " " + e.Midname).Contains(search));
            }

            var employees = await query.ToListAsync();

            ViewBag.TotalEmployees = employees.Count;
            ViewBag.Search = search;

            return View("EmployeeIndex", employees);
        }

        //выбор сотрудника из таблицы, вся страница
        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetails(int id)
        {
            var emp = await _context.Employees
                .Include(e => e.PostNavigation)
                .Include(e => e.DepartmentNavigation)
                .Include(e => e.SectionNavigation)
                .FirstOrDefaultAsync(e => e.CodeEmployee == id);

            if (emp == null)
                return NotFound();

            string tabnumStr = emp.TabNum?.ToString("D5");
            string photoFileName = $"{tabnumStr}.jpg";

            string rootPath = Directory.GetCurrentDirectory();
            string mainPath = Path.Combine(rootPath, "wwwroot", "Photo", photoFileName);
            string archivePath = Path.Combine(rootPath, "wwwroot", "Photo", "Archive", photoFileName);

            string webPhotoPath;
            if (System.IO.File.Exists(mainPath))
                webPhotoPath = Url.Content($"/Photo/{photoFileName}");
            else if (System.IO.File.Exists(archivePath))
                webPhotoPath = Url.Content($"/Photo/Archive/{photoFileName}");
            else
                webPhotoPath = Url.Content("~/images/default-profile.jpg");

            return Json(new
            {
                fullname = $"{emp.Surname} {emp.Firstname} {emp.Midname}",
                nameCh = emp.NameCh,
                tabnum = tabnumStr,
                inputDate = emp.InputDate?.ToString("dd.MM.yyyy"),
                post = emp.PostNavigation?.Postt + " " + emp.PostNavigation?.PostCh,
                department = emp.DepartmentNavigation?.DepartmentName + " " + emp.DepartmentNavigation?.DepartmentCh,
                section = emp.SectionNavigation?.SectionName + " " + emp.SectionNavigation?.SectionCh,
                photoPath = webPhotoPath
            });
        }

        // Получение полной информации о сотруднике по ID в мод окне
        [HttpGet]
        public async Task<IActionResult> GetEmployeeFullDetails(int id)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.CodeEmployee == id);
            if (emp == null) return NotFound();

            // Фото
            string photoPath;
            var fileName = emp.TabNum?.ToString("D5") + ".jpg";
            var pathMain = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", fileName);
            var pathArch = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", "Archive", fileName);

            if (System.IO.File.Exists(pathMain))
                photoPath = Url.Content($"/Photo/{fileName}");
            else if (System.IO.File.Exists(pathArch))
                photoPath = Url.Content($"/Photo/Archive/{fileName}");
            else
                photoPath = Url.Content("~/images/default-profile.jpg");

            // PartTime список с первым элементом "—"
            var partTimeList = _context.Posts
                .Select(p => new NamedId
                {
                    Code = p.CodePost,
                    Title = p.Postt + " " + p.PostCh
                }).ToList();
            partTimeList.Insert(0, new NamedId { Code = null, Title = "—" });

            // Название совмещения (или "—")
            string partTimeTitle = emp.PartTime != null
                ? _context.Posts
                    .Where(p => p.CodePost == emp.PartTime)
                    .Select(p => p.Postt + " " + p.PostCh)
                    .FirstOrDefault()
                : "—";

            // Получение категории сотрудника через таблицу Owners
            var owner = _context.Owners
                .Include(o => o.CategoryNavigation)
                .FirstOrDefault(o => o.CodeEmployee == emp.CodeEmployee);

            int? categoryCode = owner?.CodeCategory;
            string? categoryTitle = owner?.CategoryNavigation?.Category ?? "—";

           /* var categories = _context.OwnerCategories
                .Select(c => new NamedId
                {
                    Code = c.CodeCategory,
                    Title = c.Category
                }).ToList();
            categories.Insert(0, new NamedId { Code = null, Title = "—" });*/

            var ownerCategory = _context.Owners
                .Where(o => o.CodeEmployee == emp.CodeEmployee)
                .Select(o => o.CodeCategory)
                .FirstOrDefault();

            var categories = _context.OwnerCategories
                .Select(c => new NamedId
                {
                    Code = c.CodeCategory,
                    Title = c.Category + " " + c.CategoryCh
                }).ToList();

            categories.Insert(0, new NamedId { Code = null, Title = "—" });



            return Json(new
            {
                codeEmployee = emp.CodeEmployee,
                surname = emp.Surname,
                firstname = emp.Firstname,
                midname = emp.Midname,
                nameCh = emp.NameCh,
                tabNum = emp.TabNum?.ToString("D5"),
                inputDate = emp.InputDate,
                contractNumber = emp.ContractNumber,
                contractDate = emp.ContractDate,
                fired = emp.Fired ?? false,
                firingDate = emp.FiringDate,
                hazard = emp.Hazard,
                hazardDoc = emp.HazardDoc,
                sex = emp.Sex,
                birthday = emp.Birthday,
                passport = emp.Passport,
                citizenship = emp.Citizenship,
                nationality = emp.Nationality,
                address = emp.Address,
                district = emp.District,
                post = emp.Post,
                partTime = emp.PartTime,
                partTimeTitle = partTimeTitle,
                partTimeList = partTimeList,
                department = emp.Department,
                section = emp.Section,
                quota = emp.CodeQuota,
                photo = photoPath,
                category = categoryCode,
                categoryTitle = categoryTitle,
                categories = categories,
                codeCategory = ownerCategory,
                




                posts = _context.Posts.Select(p => new NamedId
                {
                    Code = p.CodePost,
                    Title = p.Postt + " " + p.PostCh
                }).ToList(),

                departments = _context.Departments.Select(d => new NamedId
                {
                    Code = d.CodeDepartment,
                    Title = d.DepartmentName + " " + d.DepartmentCh
                }).ToList(),

                sections = _context.Sections.Select(s => new NamedId
                {
                    Code = s.CodeSection,
                    Title = s.SectionName + " " + s.SectionCh
                }).ToList(),

                quotas = _context.Quotas.Select(q => new NamedId
                {
                    Code = q.CodeQuota,
                    Title = q.Quotaa != null ? q.Quotaa.ToString() : "—"
                }).ToList(),


                hazardDocs = _context.CompanyDocs.Select(h => new NamedId
                {
                    Code = h.Code,
                    Title = h.Title
                }).ToList(),

                sexes = _context.Sexes.Select(s => new NamedId
                {
                    Code = s.CodeSex,
                    Title = s.Sex + " " + s.SexCh
                }).ToList(),

                citizenships = _context.Citizenships.Select(c => new NamedId
                {
                    Code = c.CodeCitizenship,
                    Title = c.Citizenship + " " + c.CitizenshipCh
                }).ToList(),

                nationalities = _context.Nationalities.Select(n => new NamedId
                {
                    Code = n.CodeNationality,
                    Title = n.Nationality + " " + n.NationalityCh
                }).ToList(),

                districts = _context.Districts.Select(d => new NamedId
                {
                    Code = d.CodeDistrict,
                    Title = d.District + " " + d.DistrictCh
                }).ToList(),
                

            });
        }

        public class NamedId
        {
            public int? Code { get; set; }
            public string?Title { get; set; }
        }
        //Замена фото сотрудника
        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile photo, string tabNum)
        {
            if (photo == null || string.IsNullOrWhiteSpace(tabNum))
                return BadRequest("Файл или табельный номер не передан");

            if (!photo.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Разрешены только файлы .jpg");

            string paddedTabNum = int.TryParse(tabNum, out int num) ? num.ToString("D5") : tabNum;
            string fileName = $"{paddedTabNum}.jpg";

            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photo", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            string photoUrl = Url.Content($"/Photo/{fileName}");
            return Content(photoUrl);
        }


        // Сохранение изменений сотрудника
        [HttpPost]
        public async Task<IActionResult> SaveEmployee([FromBody] EmployeeViewModel updated)
        {
            if (updated == null || updated.CodeEmployee == 0)
                return BadRequest("Неверные данные");

            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.CodeEmployee == updated.CodeEmployee);
            if (emp == null) return NotFound();

            // Сохраняем старый табельный номер
            var oldTabNum = emp.TabNum;

            // Проверка на конфликт нового TabNum (если он изменился)
            if (updated.TabNum != oldTabNum)
            {
                bool tabNumExists = await _context.Employees.AnyAsync(e => e.TabNum == updated.TabNum);
                if (tabNumExists)
                {
                    var suggested = await GetSuggestedTabNum();
                    return BadRequest($"Таб. номер {updated.TabNum} уже занят. Предложение: {suggested:D5}");
                }

               
                // Переименование фото (если есть файл с прошлым табельным номером)
                var photoDir = Path.Combine("wwwroot", "Photo");
                var oldFile = Path.Combine(photoDir, $"{oldTabNum:D5}.jpg");
                var newFile = Path.Combine(photoDir, $"{updated.TabNum:D5}.jpg");
                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Move(oldFile, newFile);
                }

            }

            // Обновление полей
            emp.Surname = updated.Surname;
            emp.Firstname = updated.Firstname;
            emp.Midname = updated.Midname;
            emp.NameCh = updated.NameCh;
            emp.TabNum = updated.TabNum;
            emp.InputDate = updated.InputDate;
            emp.Post = updated.Post;
            emp.PartTime = updated.PartTime;
            emp.Department = updated.Department;
            emp.Section = updated.Section;
            emp.ContractNumber = updated.ContractNumber;
            emp.ContractDate = updated.ContractDate;
            emp.Fired = updated.Fired;
            emp.FiringDate = updated.FiringDate;
            emp.Hazard = updated.Hazard;
            emp.HazardDoc = updated.HazardDoc;
            emp.Sex = updated.Sex;
            emp.Birthday = updated.Birthday;
            emp.Passport = updated.Passport;
            emp.Citizenship = updated.Citizenship;
            emp.Nationality = updated.Nationality;
            emp.Address = updated.Address;
            emp.District = updated.District;
            emp.CodeQuota = updated.CodeQuota;

            // Обновление CodeCategory
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.CodeEmployee == updated.CodeEmployee);

            if (owner != null)
            {
                owner.CodeCategory = updated.CodeCategory;
            }
            else if (updated.CodeCategory != null)
            {
                _context.Owners.Add(new Owner
                {
                    CodeEmployee = updated.CodeEmployee,
                    CodeCategory = updated.CodeCategory.Value
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        //метод помощник для предложения табельного номера
        private async Task<int> GetSuggestedTabNum()
        {
            var used = await _context.Employees
                .Where(e => e.TabNum.HasValue)
                .Select(e => e.TabNum.Value)
                .ToListAsync();

            for (int i = 1; i < 99999; i++)
            {
                if (!used.Contains(i))
                    return i;
            }
            return 99999;
        }

        //получение зарегистрированных телефонов сотрудника для мод инфо окна
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

        //метод загрузки инфоо сотруднике для модального окна с информацией
        [HttpGet]
        public IActionResult GetEmployeeInfoModal(int id)
        {
            // Достаём сотрудника по ID
            var emp = _context.Employees
                .Include(e => e.PostNavigation)
                .Include(e => e.DepartmentNavigation)
                .Include(e => e.SectionNavigation)
                .FirstOrDefault(e => e.CodeEmployee == id);

            if (emp == null)
                return Json(null);

            // Подгружаем справочники
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
                Post = emp.PostNavigation?.Postt + " " + emp.PostNavigation?.PostCh,
                Department = emp.DepartmentNavigation?.DepartmentName + " " + emp.DepartmentNavigation?.DepartmentCh,
                Section = emp.SectionNavigation?.SectionName + " " + emp.SectionNavigation?.SectionCh,
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
                    .FirstOrDefault()
            });
        }





    }
}
