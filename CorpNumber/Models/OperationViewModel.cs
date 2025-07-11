namespace CorpNumber.Models
{
    public class OperationViewModel
    {
        public int CodeOperation { get; set; }
        public int? CodeOperType { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? OperDate { get; set; }
        public int? Number { get; set; }
        public bool? Complete { get; set; }
        public string? Comments { get; set; }
        public string? Information { get; set; }
        public int? OrderN { get; set; }
        public string? Type { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
