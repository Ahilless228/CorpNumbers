using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Router
    {
        [Key]
        public int CodeRouter { get; set; }
        public int? Number { get; set; }
        public string? Model{ get; set; }
        public string? IMEI{ get; set; }
        public string? Serial { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? Comments { get; set; }

        [ForeignKey("Number")]
        public virtual Phone? NumberPhone { get; set; }

    }
}
