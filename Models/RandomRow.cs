using System;
using System.Collections.Generic;

namespace TestApplication.Models
{
    public partial class RandomRow
    {
        public int RowId { get; set; }
        public DateOnly RandomDate { get; set; }
        public string LatinString { get; set; } = null!;
        public string RussianString { get; set; } = null!;
        public decimal EvenNumber { get; set; }
        public decimal FloatNumber { get; set; }
    }
}
