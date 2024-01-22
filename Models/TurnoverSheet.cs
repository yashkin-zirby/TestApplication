using System;
using System.Collections.Generic;

namespace TestApplication.Models
{
    public partial class TurnoverSheet
    {
        public TurnoverSheet()
        {
            TurnoverStatements = new HashSet<TurnoverStatement>();
        }

        public int SheetId { get; set; }
        public DateOnly ReportYear { get; set; }
        public string BankName { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public string FileName { get; set; } = null!;

        public virtual ICollection<TurnoverStatement> TurnoverStatements { get; set; }
    }
}
