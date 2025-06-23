namespace CorpNumber.Models
{
    public class Phone
    {
        public int CodePhone { get; set; }
        public int? Number { get; set; }
        public string? ICCID { get; set; }
        public int? CodeOwner { get; set; }
        public int? Operator { get; set; }
        public int? Tariff { get; set; }
        public int? Account { get; set; }
        public int? Status { get; set; }
        public int? Internet { get; set; }
        public short? Limit { get; set; }
        public string? Comments { get; set; }
        public bool? Corporative { get; set; }
        public bool? Router { get; set; }
        public bool? Phonebook { get; set; }
        public byte[]? SSMA_TimeStamp { get; set; }
        // Навигационные свойства:
        public virtual Operator? OperatorNavigation { get; set; }
        public virtual Tariff? TariffNavigation { get; set; }
        public virtual Status? StatusNavigation { get; set; }
        public virtual InternetService? InternetNavigation { get; set; }
    }
}
