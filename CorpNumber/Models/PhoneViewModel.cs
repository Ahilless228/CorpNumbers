namespace CorpNumber.Models
{
    public class PhoneViewModel
    {
        public int CodePhone { get; set; }
        public string ?Number { get; set; }
        public string ?ICCID { get; set; }
        public string ?Operator { get; set; }         // Название оператора
        public string ?Account { get; set; }          // Название счёта (если есть)
        public string ?Tariff { get; set; }           // Название тарифа
        public string ?Status { get; set; }           // Название состояния
        public string ?Internet { get; set; }         // Название интернет-пакета
        public short? Limit { get; set; }
        public bool Corporative { get; set; }
        public string? FullName { get; set; }        // ФИО
        public string? NameCh { get; set; }          // 姓名
    }

}
