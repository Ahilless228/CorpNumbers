using Microsoft.AspNetCore.Mvc;
using ParsingPDF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using CorpNumber.Models;
using ClosedXML.Excel;
using System.Collections.Concurrent;

namespace CorpNumber.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PdfParseController : ControllerBase
    {
        private readonly CorpNumberDbContext _context;

        public PdfParseController(CorpNumberDbContext context)
        {
            _context = context;
        }


        [HttpPost("parse")]
        public IActionResult ParsePdf([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран.");

            // Сохраняем файл во временную папку
            var tempPath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempPath))
                file.CopyTo(stream);

            // Определяем нужный парсер по имени файла
            List<ParsedReport> reports;
            var fileName = file.FileName;
            if (fileName.StartsWith("О!"))
                reports = PdfParser.Parse(tempPath);
            else if (fileName.StartsWith("MegaCom"))
                reports = PdfParserMegaCom.Parse(tempPath);
            else if (fileName.StartsWith("Beeline"))
                reports = PdfParserBeeline.Parse(tempPath);
            else
                return BadRequest("Неизвестный формат отчёта.");

            int saved = 0;
            var skipped = new List<string>();

            foreach (var parsed in reports)
            {
                if (!int.TryParse(parsed.PhoneNumber, out var phoneNumberInt))
                {
                    skipped.Add($"Некорректный номер телефона: {parsed.PhoneNumber}");
                    continue;
                }

                var phone = _context.Phones.FirstOrDefault(p => p.Number == phoneNumberInt);
                if (phone == null)
                {
                    skipped.Add($"Телефон {parsed.PhoneNumber} не найден в базе.");
                    continue;
                }

                // Проверка на дубликат
                bool reportExists = _context.ReportEntries.Any(r =>
                    r.PhoneId == phone.CodePhone && r.ReportDate == parsed.ReportDate);

                if (reportExists)
                {
                    skipped.Add($"Дубликат отчёта: PhoneId={phone.CodePhone}, Date={parsed.ReportDate:yyyy-MM-dd}");
                    continue;
                }

                var entry = new ReportEntry
                {
                    PhoneId = phone.CodePhone,
                    TotalAmount = parsed.TotalAmount,
                    ReportDate = parsed.ReportDate,
                    Services = new List<ServiceCharge>()
                };

                foreach (var (serviceName, amount) in parsed.Services)
                {
                    var service = _context.ServiceNames.FirstOrDefault(s => s.Name == serviceName);
                    if (service == null)
                    {
                        service = new ServiceName { Name = serviceName };
                        _context.ServiceNames.Add(service);
                        _context.SaveChanges();
                    }

                    entry.Services.Add(new ServiceCharge
                    {
                        ServiceNameId = service.Id,
                        Amount = amount
                    });
                }

                _context.ReportEntries.Add(entry);
                _context.SaveChanges();
                saved++;
            }

            return Ok(new
            {
                Parsed = reports.Count,
                Saved = saved,
                Skipped = skipped
            });
        }

        [HttpPost("exportExcel")]
        public IActionResult ExportExcel([FromForm] IFormFile file, [FromForm] string parseType)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран.");

            try
            {
                var tempPath = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(tempPath))
                    file.CopyTo(stream);

                List<ParsedReport> reports;
                var fileName = file.FileName;
                string reportType;
                if (fileName.StartsWith("О!"))
                {
                    reports = PdfParser.Parse(tempPath);
                    reportType = "О!";
                }
                else if (fileName.StartsWith("MegaCom"))
                {
                    reports = PdfParserMegaCom.Parse(tempPath);
                    reportType = "MegaCom";
                }
                else if (fileName.StartsWith("Beeline"))
                {
                    reports = PdfParserBeeline.Parse(tempPath);
                    reportType = "Beeline";
                }
                else
                {
                    return BadRequest("Неизвестный формат отчёта.");
                }

                string reportDateStr = reports.Count > 0
                    ? reports[0].ReportDate.ToString("yyyy.MM")
                    : DateTime.Now.ToString("yyyy.MM");


                using var ms = new MemoryStream();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Отчёт");
                    worksheet.Cell(1, 1).Value = "Номер";
                    worksheet.Cell(1, 2).Value = "Начислений";
                    for (int i = 0; i < reports.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = $"996{reports[i].PhoneNumber}";
                        worksheet.Cell(i + 2, 2).Value = reports[i].TotalAmount;
                        worksheet.Cell(i + 2, 2).Style.NumberFormat.Format = "#,##0.00";
                    }
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(ms);
                }
                ms.Position = 0;

                var fileBytes = ms.ToArray();
                var result = new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                var fullfileName = $"Parsing_{reportType}_{reportDateStr}.xlsx";
                var encodedFileName = Uri.EscapeDataString(fullfileName);

                // Устанавливаем только корректный заголовок вручную
                Response.Headers["Content-Disposition"] =
                    $"attachment; filename=\"{fullfileName}\"; filename*=UTF-8''{encodedFileName}";

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при создании Excel-файла: {ex.Message}");
            }
        }
    }
}