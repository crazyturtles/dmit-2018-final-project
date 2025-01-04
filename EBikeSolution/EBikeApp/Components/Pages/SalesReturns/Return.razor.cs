using EBikeLibrary.ViewModels;
using Microsoft.AspNetCore.Components;
using EBikeLibrary.BLL.SaleReturnServices;

namespace EBikeApp.Components.Pages.SalesReturns
{
    public partial class Return
    {
        #region Properties
        [Inject] protected ReturnService ReturnService { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        #endregion

        #region fields
        private int invoiceid;
        private int refundQuantity;
        private string reason;
        private SaleView currentSale = new();
        private List<SaleDetailView> saleDetails = new List<SaleDetailView>();
        private SaleDetailView currentSaleDetail = new();
        private string couponCode;
        private int couponDiscount;
        private int subTotal;
        private int tax;
        private int discount;
        private int total;
        private SaleRefundView currentRefund = new();
        private List<SaleRefundDetailView> saleRefundDetails = new List<SaleRefundDetailView>();
        #endregion

        private async void LookupSale()
        {
            currentRefund = ReturnService.GetSaleRefund(invoiceid);
            saleRefundDetails = ReturnService.GetSaleDetailsRefund(invoiceid);
            GetTotals();
        }
        private void Clear()
        {
            saleRefundDetails.Clear();
            currentRefund = new();
            subTotal = 0;
            tax = 0;
            discount = 0;
            total = 0;
        }
        private void Refund()
        {

        }
        private void GetTotals()
        {
            subTotal = 0;
            tax = 0;
            discount = 0;
            total = 0;
            foreach (var SaleRefundDetailView in saleRefundDetails)
            {
                subTotal += Convert.ToInt32(@SaleRefundDetailView.SellingPrice) * @SaleRefundDetailView.Quantity;
                subTotal += Convert.ToInt32(SaleRefundDetailView.SellingPrice);
                tax += (Convert.ToInt32(SaleRefundDetailView.SellingPrice) * SaleRefundDetailView.Quantity) / 20;
                discount += ((Convert.ToInt32(SaleRefundDetailView.SellingPrice) * SaleRefundDetailView.Quantity) / 100 * couponDiscount);
            }
            total = subTotal + tax - discount;
        }

    }
}
