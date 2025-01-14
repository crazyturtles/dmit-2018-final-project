namespace SalesReturns.ViewModels
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