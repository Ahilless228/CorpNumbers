using ParsingPDF;
using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ParsingPDF
{
    public class PdfParserMegaCom
    {
        public static List<ParsedReport> Parse(string pdfPath)
        {
            var result = new List<ParsedReport>();

            using var pdf = PdfDocument.Open(pdfPath);
            // Собираем текст, заменяя переносы строк на пробелы для "плотного" формата
            var rawText = string.Join(" ", pdf.GetPages().Select(p => p.Text.Replace("\n", " ")));

            // Извлекаем дату периода (например, "период с 01.05.2025 по 31.05.2025")
            var periodMatch = Regex.Match(rawText, @"период с (\d{2}\.\d{2}\.\d{4}) по (\d{2}\.\d{2}\.\d{4})");
            DateTime reportDate = periodMatch.Success
                ? DateTime.ParseExact(periodMatch.Groups[2].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture)
                : new DateTime(2025, 5, 31);

            // Разбиваем по номерам (каждый блок начинается с 996XXXXXXXXX)
            var msisdnMatches = Regex.Matches(rawText, @"996\d{9}");
            var blocks = Regex.Split(rawText, @"(?=996\d{9})").Where(b => b.Contains("996")).ToList();

            foreach (var block in blocks)
            {
                var msisdnMatch = Regex.Match(block, @"996\d{9}");
                if (!msisdnMatch.Success)
                    continue;

                var msisdn = msisdnMatch.Value;
                var report = new ParsedReport
                {
                    PhoneNumber = msisdn.Substring(msisdn.Length - 9),
                    ICCID = "",
                    ReportDate = reportDate,
                    Services = new()
                };

                // Удаляем всё до MSISDN, чтобы избежать дубликатов
                var cleanBlock = block.Substring(block.IndexOf(msisdn) + msisdn.Length);

                // Новый паттерн: ищем название услуги и сумму перед "шт", "мин", "Мб" или в конце
                var servicePattern = @"([А-Яа-яA-Za-z\s\-\.\+]+?)(\d{1,6}[.,]\d{2})(шт|мин|Мб)";
                var matches = Regex.Matches(cleanBlock, servicePattern);

                foreach (Match m in matches)
                {
                    var name = m.Groups[1].Value.Trim();
                    var amountStr = m.Groups[2].Value.Replace(',', '.');
                    if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) && amount != 0)
                    {
                        if (!name.ToLower().Contains("итого"))
                            report.Services.Add((name, amount));
                    }
                }

                // Итоговая сумма — из строки "ИТОГО", если есть
                var totalMatch = Regex.Match(cleanBlock, @"ИТОГО[^0-9]*([\d.,]+)", RegexOptions.IgnoreCase);
                if (totalMatch.Success)
                {
                    var totalStr = totalMatch.Groups[1].Value.Replace(',', '.');
                    if (decimal.TryParse(totalStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var total))
                        report.TotalAmount = total;
                    else
                        report.TotalAmount = report.Services.Sum(s => s.Amount);
                }
                else
                {
                    report.TotalAmount = report.Services.Sum(s => s.Amount);
                }

                if (report.Services.Count > 0)
                    result.Add(report);
            }

            Console.WriteLine($"✅ MegaCom: найдено записей: {result.Count}");
            return result;
        }
    }
}
