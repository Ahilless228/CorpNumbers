using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class SimCard
    {
        [Key]
        public int CodeCard { get; set; }
        public string ICCID { get; set; }
        public int? Operator { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public int? Operation { get; set; }
        public bool? Incomplete { get; set; }
        public string Comments { get; set; }
        public byte[] SSMA_TimeStamp { get; set; }
    }

}
