using Microsoft.AspNetCore.Components;
using EBikeLibrary.ViewModels;
using EBikeLibrary.BLL;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.IdentityModel.Tokens;
using EBikeLibrary.Entities;

namespace EBikeApp.Components.Pages.Receiving
{
    public partial class ReceivingComponent
    {
        #region Feedback & Error Messages
        //  placeholder for feedback message
        private string feedbackMessage;

        //  placeholder for error messasge
        private string? errorMessage;

        //  return has feedback
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);

        //  return has error
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        //  used to display any collection of errors on web page
        //  whether the errors are generated locally or come from the class library
        private List<string> errorDetails = new();
        #endregion


        #region Properties
        //  The receiving service
        [Inject] 
        protected ReceivingService ReceivingService { get; set; }

        //   Injects the NavigationManager dependency
        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        protected List<PurchaseOrderView> PurchaseOrders { get; set; } = new ();

        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {

                //  reset the error detail list
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = String.Empty;

                await InvokeAsync(StateHasChanged);

                PurchaseOrders = ReceivingService.GetOutstandingOrder();
                
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }

                errorMessage = $"{errorMessage}Unable to search for purchase orders";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }

        }
        private void Receiving_FetchOrderDetails(int PurchaseOrderID)
        {
            NavigationManager.NavigateTo($"/Receiving/EditOutstandingOrderDetails/{PurchaseOrderID}");
        }
    }
}
