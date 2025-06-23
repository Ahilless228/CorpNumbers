using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Quota
    {
        public int CodeQuota { get; set; }
        [Column("Quota")]
        public short? Quotaa { get; set; }
    }

}
