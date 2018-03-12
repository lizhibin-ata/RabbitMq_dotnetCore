
using System;
using System.ComponentModel.DataAnnotations;

namespace MQ.Models
{
    public class PurchaseOrder
    {
        public decimal AmountToPay;
        public string PoNumber;
        public string CompanyName;
        public int PaymentDayTerms;
    }
}
