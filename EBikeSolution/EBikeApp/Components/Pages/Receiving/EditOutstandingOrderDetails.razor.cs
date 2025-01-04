using Microsoft.AspNetCore.Components;
using EBikeLibrary.ViewModels;
using EBikeLibrary.BLL;
using Microsoft.AspNetCore.Components.Forms;
using EBikeLibrary.Entities;
using Microsoft.IdentityModel.Tokens;
using EBikeApp.Components.Account.Pages.Manage;

namespace EBikeApp.Components.Pages.Receiving
{
    public partial class EditOutstandingOrderDetails
    {
        #region Fields 

        private PurchaseOrderView purchaseOrder = new();

        private ReceivingDetailView receiveDetails = new();

        private List<ReceivingDetailView> receivingDetailView = new();

        private List<UnorderedReturnItemView> unorderedReturnItems = new();

        private string forceCloseReason = "";

        #endregion
        #region Validation 
        private string closeButtonText = "Force Close";

        private EditContext editContext;

        private bool disableSaveButton => !editContext.IsModified() || !editContext.Validate();

        //  used to store the validation message
        private ValidationMessageStore messageStore;
        #endregion
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


        /// The show dialog

        private bool showDialog = false;

        /// The dialog message

        private string dialogMessage = string.Empty;
        /// The should invoice line be remove



        /// The dialog completion source
        private TaskCompletionSource<bool?> dialogCompletionSource;

        private async Task ShowDialog()
        {
            dialogMessage = "Do you wish to close the Receiving Order Detail Editor?";
            showDialog = true;
        }
        #endregion

        #region Properties
        //  The receiving service
        [Inject]
        protected ReceivingService ReceivingService { get; set; }
        //   Injects the NavigationManager dependency
        [Inject]
        protected NavigationManager NavigationManager { get; set; }
        [Parameter] public int PurchaseOrderNumber { get; set; }
        [Parameter] public string? VendorName { get; set; }
        [Parameter] public string? VendorPhone { get; set; }

        [Parameter] public int PurchaseOrderID { get; set; }


        //protected PurchaseOrderView orderDetails { get; set; }
        //private List<ReceivingDetailView> receivingOrderDetails = new();
        private List<UnorderedReturnItemView> unOrderedReturnItemView = new();

        #endregion
        protected override async Task OnInitializedAsync()
        {
            {
                await base.OnInitializedAsync();

                try
                {
                    if (PurchaseOrderID != null)
                    {
                        PurchaseOrderID = PurchaseOrderID;
                        purchaseOrder.VendorName = VendorName;
                        purchaseOrder.VendorPhone = VendorPhone;

                        receivingDetailView = ReceivingService.Receiving_FetchOrderDetails(PurchaseOrderID);
                        
                       // unorderedReturnItems = ReceivingService.GetUnorderedReturnItems(PurchaseOrderID);
                    
                    await InvokeAsync(StateHasChanged);
                    }
                }
                catch (AggregateException ex)
                {
                    //  have a collection of error
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage = $"{errorMessage}{Environment.NewLine}";
                    }

                    errorMessage = $"{errorMessage}Unable to search for invoice";
                    foreach (var error in ex.InnerExceptions)
                    {
                        errorDetails.Add(error.Message);
                    }
                }

            }
        }
        private async Task ReceiveOrder()
        {
            
             ReceivingService.Receiving_Receive(PurchaseOrderID, receivingDetailView, unorderedReturnItems, forceCloseReason);
        }

        private async Task ForceCloseOrder()
        {
            
            ReceivingService.Receiving_ForceClose(PurchaseOrderID, receivingDetailView, unorderedReturnItems, "");
        }
        private async Task InsertUnorderItem()
        {
            try
            {
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = String.Empty;

                unorderedReturnItems.Add(new UnorderedReturnItemView());

                await InvokeAsync(StateHasChanged);
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

                    errorMessage = $"{errorMessage}Unable to insert unorder item";
                    foreach (var error in ex.InnerExceptions)
                    {
                        errorDetails.Add(error.Message);
                    }
                }

            }
        }

        private void ClearUnorderItem()
        {
            try
            {
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = String.Empty;

                if (unorderedReturnItems.Count > 0)
                {
                    var lastItem = unorderedReturnItems.Last();
                    lastItem.Description = string.Empty;
                    lastItem.Quantity = 0;
                    lastItem.VSN = string.Empty;
                }
                
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

                    errorMessage = $"{errorMessage}Unable to clear unorder item";
                    foreach (var error in ex.InnerExceptions)
                    {
                        errorDetails.Add(error.Message);
                    }
                }

            }

        }

        private async Task DeleteUnorderItem(UnorderedReturnItemView item)
        {
            try
            {
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = String.Empty;

                unorderedReturnItems.Remove(item);

                await InvokeAsync(StateHasChanged);
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

                    errorMessage = $"{errorMessage}Unable to delete unorder item";
                    foreach (var error in ex.InnerExceptions)
                    {
                        errorDetails.Add(error.Message);
                    }
                }

            }
        }
    } 
}

