﻿using System.ComponentModel.DataAnnotations;

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
    }

}
