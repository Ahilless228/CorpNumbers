namespace CorpNumber.Models
{
    public class InternetService
    {
        public int CodeServ { get; set; }
        public string ?Service { get; set; }

        public virtual ICollection<Phone>? Phones { get; set; }
    }
}
