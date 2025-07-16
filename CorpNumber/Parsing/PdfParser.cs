using ParsingPDF;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
namespace ParsingPDF
{
    public class ParsedReport
    {
        public string PhoneNumber { get; set; }
        public string ICCID { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ReportDate { get; set; }
        public List<(string ServiceName, decimal Amount)> Services { get; set; } = new();
    }

    public class PdfParser
    {
        public static List<ParsedReport> Parse(string pdfPath)
        {
            var result = new List<ParsedReport>();

            using var pdf = PdfDocument.Open(pdfPath);

            DateTime reportDate = DateTime.MinValue;
            bool reportDateFound = false;

            foreach (var page in pdf.GetPages())
            {
                // Убираем переносы строк и разбиваем по длинным линиям тире
                var text = page.Text.Replace("\r", "").Replace("\n", "");

                // Извлекаем дату отчёта только один раз для всего отчёта
                if (!reportDateFound)
                {
                    var dateMatch = Regex.Match(text, @"Начальная дата и времяКонечная дата и время\s*([0-9]{2}\.[0-9]{2}\.[0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2})");
                    if (dateMatch.Success)
                    {
                        if (DateTime.TryParseExact(
                                dateMatch.Groups[1].Value,
                                "dd.MM.yyyy HH:mm:ss",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out reportDate))
                        {
                            reportDateFound = true;
                        }
                        else
                        {
                            Console.WriteLine($"❌ Не удалось распарсить дату: {dateMatch.Groups[1].Value}");
                        }
                    }
                }

                var blocks = Regex.Split(text, @"-{20,}")
                                  .Select(b => b.Trim())
                                  .Where(b => !string.IsNullOrWhiteSpace(b))
                                  .ToList();

                foreach (var block in blocks)
                {
                    Console.WriteLine("==== BLOCK ====");
                    Console.WriteLine(block);
                    

                    // Новый паттерн для ICCID и MSISDN
                    var iccMsisdnMatch = Regex.Match(block, @"(?<!\d)(\d{18,19})ICCMSISDN(\d{12})(?!\d)");
                    if (!iccMsisdnMatch.Success)
                    {
                        Console.WriteLine("❌ Блок пропущен: не найден ICCID или MSISDN");
                        continue;
                    }

                    var iccid = iccMsisdnMatch.Groups[1].Value;
                    var msisdn = iccMsisdnMatch.Groups[2].Value;

                    // Оставляем только последние 9 цифр номера
                    if (msisdn.Length >= 9)
                        msisdn = msisdn.Substring(msisdn.Length - 9);
                    else
                    {
                        Console.WriteLine($"Некорректный номер телефона: {msisdn}. Запись пропущена.");
                        continue;
                    }

                    // Используем найденную ранее дату
                    if (!reportDateFound)
                    {
                        Console.WriteLine("❌ Не найдена строка 'Начальная дата и время' во всём отчёте. Запись пропущена.");
                        continue;
                    }

                    // Проверяем наличие услуг
                    if (!block.Contains("Начисления") && !block.Contains("Для абонента №"))
                    {
                        Console.WriteLine("❌ Блок пропущен: не найдены услуги");
                        continue;
                    }

                    // Найдём блок с услугами
                    var servicesStart = block.IndexOf("Для абонента №", StringComparison.Ordinal);
                    if (servicesStart == -1)
                        servicesStart = block.IndexOf("Начисления", StringComparison.Ordinal);
                    if (servicesStart == -1)
                        servicesStart = 0;

                    var servicesEnd = block.IndexOf("Итого начислений с учетом НДС и НП(сом):", StringComparison.Ordinal);
                    if (servicesEnd == -1)
                        servicesEnd = block.Length;

                    var servicesText = block.Substring(servicesStart, servicesEnd - servicesStart);

                    var services = new List<(string, decimal)>();

                    // Извлекаем услуги: Название + сумма через "0" (без пробела!)
                    var serviceMatches = Regex.Matches(servicesText, @"(.+?)0(\d{1,5}[.,]\d{2})(?=\D|$)");
                    foreach (Match m in serviceMatches)
                    {
                        var name = m.Groups[1].Value.Trim();
                        var amountStr = m.Groups[2].Value.Replace(',', '.');

                        if (decimal.TryParse(amountStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount) && amount != 0)
                        {
                            if (name.Contains("Для абонента")) continue;
                            if (name.Contains("Начисления")) continue;
                            if (name.Contains("С корп счета")) continue;
                            if (name.Contains("С личного счета")) continue;
                            services.Add((name, amount));
                        }
                    }

                    // Итоговая сумма
                    var totalMatch = Regex.Match(block, @"Итого начислений с учетом НДС и НП\(сом\):\s*0\s*(\d{1,5}[.,]\d{2})");
                    decimal total = 0;
                    if (totalMatch.Success)
                        decimal.TryParse(totalMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out total);

                    // Добавление в результат
                    var parsed = new ParsedReport
                    {
                        PhoneNumber = msisdn,
                        ICCID = iccid,
                        TotalAmount = total,
                        ReportDate = reportDate,
                        Services = services
                    };

                    result.Add(parsed);
                }
            }

            Console.WriteLine($"✅О! Распарсено записей: {result.Count}");

            // Проверка: есть ли хотя бы одна услуга
            if (!result.Any(r => r.Services.Count > 0))
            {
                Console.WriteLine("❌ Не распознано ни одной услуги. Парсинг прерван.");
                Environment.Exit(1); // Завершить выполнение программы с ошибкой
            }

            result = result
                .GroupBy(r => new { r.PhoneNumber, r.ICCID, r.TotalAmount, r.ReportDate })
                .Select(g => g.First())
                .ToList();

            return result;
        }
    }
}