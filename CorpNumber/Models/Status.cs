namespace CorpNumber.Models
{
    public class Status
    {
        public int Code { get; set; }
        public string ?StatusText { get; set; }

        public virtual ICollection<Phone>? Phones { get; set; }
    }
}
