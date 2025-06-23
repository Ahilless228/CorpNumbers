using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Quota
    {
        [Key]
        public int CodeQuota { get; set; }
        [Column("Quota")]
        public short? Quotaa { get; set; }
    }

}
