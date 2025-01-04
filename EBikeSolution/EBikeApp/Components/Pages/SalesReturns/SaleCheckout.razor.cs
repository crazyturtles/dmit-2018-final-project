using EBikeLibrary.ViewModels;
using Microsoft.AspNetCore.Components;
using EBikeLibrary.BLL.SaleReturnServices;

namespace EBikeApp.Components.Pages.SalesReturns
{
    public partial class SaleCheckout
    {

        #region Properties
        [Inject] protected SaleService SaleService { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        #endregion
        #region fields
        private List<CategoryView> CategoryList { get; set; } = new();
        private List<PartView> PartList = new List<PartView>();
        private PartView currentPart = new();
        private int partQuantity;
        private SaleView currentSale = new();
        private List<SaleDetailView> saleDetails = new List<SaleDetailView>();
        private SaleDetailView currentSaleDetail = new();
        private SaleDetailView removeSaleDetail = new();
        private string couponCode;
        private int couponDiscount;
        private int categoryid;
        private int partid;
        private int subTotal;
        private int tax;
        private int discount;
        private int total;
        private string payment;

        #endregion

        #region Validation
        #endregion

        #region Feedback and error messages
        private string feedbackMessage;
        private string? errorMessage;
        //private bool hasFeedback => !string.IsNullOrEmpty(feedbackMessage);
        //private bool hasError => !string.IsNullOrEmpty(errorMessage);
        private List<string> errorDetails = new();
        #endregion
        private void LookupCat()
        {
            CategoryList = SaleService.GetCategories();
        }
        private void LookupPart() 
        {
            PartList = SaleService.GetParts(categoryid);
        }
        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }
        private async Task Add()
        {
            
            PartView part = PartList.Where(x => x.PartID == partid)
                .Select(x => x).FirstOrDefault();
            SaleDetailView detailCheck = saleDetails.Where(x => x.PartID == part.PartID).Select(x => x).FirstOrDefault();                SaleDetailView saleDetail = new SaleDetailView();
            saleDetail.PartID = part.PartID;
            saleDetail.Description = part.Description;
            saleDetail.Quantity = partQuantity;
            saleDetail.SellingPrice = part.SellingPrice;
            saleDetail.Total = part.SellingPrice * partQuantity;
            if (detailCheck == null) {            
                saleDetails.Add(saleDetail);
                await InvokeAsync(StateHasChanged); 
            }                
            GetTotals();
        }
        private void RemoveFromCart()
        {
            saleDetails.Remove(removeSaleDetail);
        }
        private void RefreshTotal()
        {
            currentSaleDetail.Total = currentPart.SellingPrice * partQuantity;
        }
        private void Checkout()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            try
            {
                currentSale = SaleService.Checkout(currentSale, saleDetails);
                feedbackMessage = "Transaction complete";
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = ex.Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = ex.Message;
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}couldn't complete transaction";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }
        private void GetTotals()
        {
            subTotal = 0;
            tax = 0;
            discount = 0;
            total = 0;
            foreach (var SaleDetailView in saleDetails)
            {
               // subTotal += Convert.ToInt32(SaleDetailView.Quantity * SaleDetailView.SellingPrice);
                subTotal += Convert.ToInt32(SaleDetailView.SellingPrice) * SaleDetailView.Quantity;
                tax += (Convert.ToInt32(SaleDetailView.SellingPrice) * SaleDetailView.Quantity) / 20;
                discount +=  ((Convert.ToInt32(SaleDetailView.SellingPrice) * SaleDetailView.Quantity) / 100 * couponDiscount);
                //discount += 
                total = subTotal + tax - discount;
            }
            
        }
        private void VerifyCoupon()
        {
            couponDiscount = SaleService.GetCoupon(couponCode);
            GetTotals();
        }
        private void Clear()
        {
            subTotal = 0;
            tax = 0;
            discount = 0;
            total = 0;
            couponDiscount = 0;
            saleDetails = new List<SaleDetailView>();
        }


    }
}
