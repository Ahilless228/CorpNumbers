namespace CorpNumber.Models
{
    public class SectionViewModel
    {
        public int CodeSection { get; set; }
        public string? Section { get; set; }
        public string? SectionCh { get; set; }
        public string? DepartmentName { get; set; } // Department + DepartmentCh
        public bool? Actuality { get; set; }
    }
}
