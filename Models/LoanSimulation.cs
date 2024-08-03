using System;
using System.Collections.Generic;

namespace credito.Models
{
    public class LoanSimulation
    {
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int NumberOfPayments { get; set; }
        public List<AmortizationItem> AmortizationSchedule { get; set; }

    }
    public class AmortizationItem
    {
        public int PaymentNumber { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal Interest { get; set; }
        public decimal Aporte { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal Pagado { get; set; }

        public decimal AcumuladorInteres { get; set; }
    }
}
