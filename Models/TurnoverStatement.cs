using System;
using System.Collections.Generic;

namespace TestApplication.Models
{
    public partial class TurnoverStatement
    {
        public int StatementId { get; set; }
        public decimal AccountCode { get; set; }
        public int TurnoverSheet { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public virtual TurnoverSheet TurnoverSheetNavigation { get; set; } = null!;
    }
}
