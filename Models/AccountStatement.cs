using System;
using System.Collections.Generic;

namespace TestApplication.Models
{
    public partial class AccountStatement
    {
        public int Statement { get; set; }
        public string AccountType { get; set; } = null!;
        public decimal OpeningBalance { get; set; }

        public virtual TurnoverStatement StatementNavigation { get; set; } = null!;
    }
}
