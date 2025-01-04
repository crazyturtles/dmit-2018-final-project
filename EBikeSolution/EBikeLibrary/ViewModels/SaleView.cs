using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBikeLibrary.ViewModels
{
    public class SaleView
    {
        public int EmployeeID { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set; }
        public int CouponID { get; set; }
        public int DiscountPercent { get; set; }
        public string PaymentType { get; set; }
    }
}
