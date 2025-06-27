using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class CompanyDocs
    {
        [Key]
        public int Code { get; set; }
        public string ?Type { get; set; }
        public string ?Title { get; set; }
        public short? Number { get; set; }
        public DateTime? DocDate { get; set; }
    }

}
