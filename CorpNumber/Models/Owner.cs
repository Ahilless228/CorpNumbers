using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Owner
    {
        [Key]
        public int CodeOwner { get; set; }
        public int? CodeCategory { get; set; }
        public int? CodeEmployee { get; set; }
        public int? CodeTempOwner { get; set; }
        public int? CodeOthers { get; set; }
        public int? CodeStationary { get; set; }

        
        public virtual OwnerCategory? CategoryNavigation { get; set; }
        public virtual ICollection<Phone> Phones { get; set; } = new List<Phone>();
        [ForeignKey("CodeEmployee")]
        public virtual Employee? EmployeeNavigation { get; set; }
        [ForeignKey("CodeTempOwner")]
        public virtual TempOwners? TempOwnerNavigation { get; set; }

    }

}
