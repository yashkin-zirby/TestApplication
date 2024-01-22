using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication.Models
{
    public class AccountingView
    {
        public int TurnoverSheetId { get; set; }
        public decimal Code { get; set; }
        public decimal OpeningBalanceActive { get; set; }
        public decimal OpeningBalancePassive { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        [NotMapped]
        public decimal ClosingBalanceActive { 
            get { 
                return OpeningBalanceActive > 0? OpeningBalanceActive + Debit - Credit : 0;
            } 
        }
        [NotMapped]
        public decimal ClosingBalancePassive {
            get
            {
                return ClosingBalanceActive > 0 ? ClosingBalanceActive + Credit - Debit : 0;
            }
        }
    }
}
