namespace CorpNumber.Models
{
    public class Tariff
    {
        public int CodeTariff { get; set; }
        public string ?Title { get; set; }
        public string ?TitleCh { get; set; }
        public int? Operator { get; set; }
        public int? Charges { get; set; }
        public int? InternetService { get; set; }
        public bool? Smartphone { get; set; }
        public bool? Actuality { get; set; }
        public string ?Comments { get; set; }
        public byte[] ?SSMA_TimeStamp { get; set; }

        public virtual ICollection<Phone>? Phones { get; set; } // 👈 связь
    }

}
