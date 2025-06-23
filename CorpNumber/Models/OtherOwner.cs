using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class OtherOwner
    {
        [Key]
        public int CodeOthers { get; set; }
        public string Title { get; set; }
        public string TitleCh { get; set; }
        public int? OtherCategory { get; set; }
    }

}
