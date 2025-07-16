using ParsingPDF;
using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace ParsingPDF
{
    public class PdfParserBeeline
    {
        public static List<ParsedReport> Parse(string pdfPath)
        {
            var result = new List<ParsedReport>();

            using var pdf = PdfDocument.Open(pdfPath);
            var fullText = string.Join("\n", pdf.GetPages().Select(p => p.Text.Replace("\r", "").Replace("\n", " ")));

            // Извлекаем дату окончания периода
            var periodMatch = Regex.Match(fullText, @"Расчетный период\s+(\d{2}-\d{2}-\d{4}) - (\d{2}-\d{2}-\d{4})");
            DateTime reportDate = periodMatch.Success
                ? DateTime.ParseExact(periodMatch.Groups[2].Value, "dd-MM-yyyy", CultureInfo.InvariantCulture)
                : new DateTime(2025, 5, 31);

            // Разбиваем по "Телефон996..."
            var blocks = Regex.Split(fullText, @"(?=Телефон996\d{9})")
                              .Where(b => b.Contains("Телефон996"))
                              .ToList();

            foreach (var block in blocks)
            {
                var msisdnMatch = Regex.Match(block, @"Телефон996(\d{9})");
                if (!msisdnMatch.Success) continue;

                var msisdn = msisdnMatch.Groups[1].Value;

                // 1. Парсим итог
                var totalMatch = Regex.Match(block, @"ИТОГО начислений с учетом НДС и НП \(сом\):\s*([\d.,]+)");
                if (!totalMatch.Success) continue;

                var totalStr = totalMatch.Groups[1].Value.Replace(',', '.');
                if (!decimal.TryParse(totalStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var total)) continue;

                var report = new ParsedReport
                {
                    PhoneNumber = msisdn,
                    ICCID = "",
                    ReportDate = reportDate,
                    Services = new(),
                    TotalAmount = total
                };

                // 2. Ежемесячные тарифы
                var monthlyMatches = Regex.Matches(block, @"([А-ЯA-Za-z0-9_ \-]+?)\d{2}/\d{2}/\d{4} - \d{2}/\d{2}/\d{4}\s*([\d.,]{2,})");
                foreach (Match m in monthlyMatches)
                {
                    var name = m.Groups[1].Value.Trim();
                    var amountStr = m.Groups[2].Value.Replace(',', '.');
                    if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) && amount != 0)
                        report.Services.Add((name, amount));
                }

                // 3. Построчные начисления
                var lineServiceMatches = Regex.Matches(block, @"([А-ЯA-Za-z0-9_()\-\/:\+ ]+?)\s+([\d]{1,6}[.,]\d{2})(?=\D|$)");
                foreach (Match m in lineServiceMatches)
                {
                    var name = m.Groups[1].Value.Trim();
                    var amountStr = m.Groups[2].Value.Replace(',', '.');

                    if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) && amount != 0)
                    {
                        if (!string.IsNullOrWhiteSpace(name))
                            report.Services.Add((name, amount));
                    }
                }

                // 4. Удалим дубли
                report.Services = report.Services
                    .GroupBy(s => s.Item1)
                    .Select(g => g.First())
                    .ToList();

                result.Add(report);
            }

            Console.WriteLine($"✅ Beeline: найдено записей: {result.Count}");
            return result;
        }
    }
}
