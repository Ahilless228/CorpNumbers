namespace CorpNumber.Models
{
    public class Operator
    {
        public int CodeOperator { get; set; }
        public string ?Title { get; set; }
        public string ?Company { get; set; }
        public string ?ICCIDCode { get; set; }
        public string ?Address { get; set; }
        public string ?Manager1Name { get; set; }
        public string ?Manager1Phone { get; set; }
        public string ?Manager2Name { get; set; }
        public string ?Manager2Phone { get; set; }
        public string ?Manager3Name { get; set; }
        public string ?Manager3Phone { get; set; }

        public virtual ICollection<Phone>? Phones { get; set; } // 👈 связь
    }

}
