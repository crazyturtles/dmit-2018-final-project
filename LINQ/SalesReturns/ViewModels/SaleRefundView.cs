namespace SalesReturns.ViewModels
{
    public class SaleRefundView
    {
        public int SaleID { get; set; }
        public int EmployeeID { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set; }
        public int DiscountPercent { get; set; }
    }
}
