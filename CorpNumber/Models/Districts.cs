using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Districts
    {
        [Key]
        public int CodeDistrict { get; set; }
        public string? District { get; set; }
        public string? DistrictCh { get; set; }
        
       
    }
}
