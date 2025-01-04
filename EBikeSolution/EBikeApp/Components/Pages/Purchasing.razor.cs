using EBikeLibrary.BLL;
using EBikeLibrary.ViewModels.Purchasing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace EBikeApp.Components.Pages
{
    public partial class Purchasing
    {
        #region Services
        [Inject]
        protected PurchasingService PurchasingService { get; set; }
        #endregion

        #region Messaging and Error Handling
        private string feedBackMessage { get; set; }
        private string feedback;
        private string errorMessage { get; set; }
        //a get property that returns the result of the lamda action
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
        private bool hasFeedBack => !string.IsNullOrWhiteSpace(feedBackMessage);
        // error details
        private List<string> errorDetails = new();
        // error list for collection 
        private List<Exception> errorList { get; set; } = new();
        #endregion

        #region Fields and Properties

        private List<ItemView> items { get; set; } = new();
        private PurchaseOrderView PO = new();
        private List<PurchaseOrderDetailView> POD = new();

        //variables used to collect data from the web page form
        private int vendorID { get; set; }
        private string selectedVendorPhone;
        private string selectedVendorCity;
        private List<VendorView> vendorsList { get; set; }

        #region Purchase Order Variables

        private int POid { get; set; }
        private int POnumber { get; set; }
        private int POvendorID { get; set; }
        private decimal POsubTotal { get; set; }
        private decimal POgst { get; set; }
        private decimal POtotal { get; set; }
        private int POemployeeID { get; set; }

        #endregion

        #region Purchase Order Detail Vairables

        private int PODpartID { get; set; }
        private string PODdescription { get; set; }
        private int PODqoh { get; set; }
        private int PODrol { get; set; }
        private int PODqoo { get; set; }
        private int PODqto { get; set; }
        private decimal PODprice { get; set; }

        #endregion

        #endregion

        #region code

        protected override async Task OnInitializedAsync()
        {
            vendorsList = PurchasingService.GetVendors();

            LoadInventory();
            await base.OnInitializedAsync();
        }

        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

        private void ClearMessages()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedBackMessage = string.Empty;
            errorList.Clear();
            StateHasChanged();
        }

        private void Clear()
        {
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedBackMessage = string.Empty;
            errorList.Clear();

            vendorID = 0;
            selectedVendorPhone = string.Empty;
            selectedVendorCity = string.Empty;

            POnumber = 0;
            POsubTotal = 0;
            POgst = 0;
            POtotal = 0;

            POD.Clear();
            items.Clear();

            StateHasChanged();
        }

        private void LoadInventory()
        {
            try
            {
                var allItems = PurchasingService.GetInventory(vendorID);
                items = allItems.Where(inventoryItem =>
                    !POD.Any(orderDetail => orderDetail.PartID == inventoryItem.PartID)).ToList();

                StateHasChanged();
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }

        private void CalculateTotals()
        {
            try
            {
                const decimal gstRate = 0.05m;

                POsubTotal = POD.Sum(detail => detail.QTO * detail.Price);
                POgst = POsubTotal * gstRate;
                POtotal = POsubTotal + POgst;

                StateHasChanged();
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }

        private void ClearOrderDetails()
        {
            POD.Clear();
            POsubTotal = 0;
            POgst = 0;
            POtotal = 0;
            POnumber = 0;
        }

        private List<PurchaseOrderDetailView> RemoveDuplicates(List<PurchaseOrderDetailView> details)
        {
            return details
                .GroupBy(detail => detail.PartID)
                .Select(g => g.Last())
                .ToList();
        }

        #endregion

        #region Find Order

        public async Task FindOrder()
        {
            try
            {
                ClearMessages();
                if (vendorID > 0)
                {
                    items = PurchasingService.GetInventory(vendorID);
                }
                else
                {
                    POD.Clear();
                    items.Clear();
                    selectedVendorPhone = string.Empty;
                    selectedVendorCity = string.Empty;

                    POnumber = 0;
                    POsubTotal = 0;
                    POgst = 0;
                    POtotal = 0;
                    POid = 0;
                }

                var selectedVendor = vendorsList.FirstOrDefault(v => v.VendorID == vendorID);
                if (selectedVendor != null)
                {
                    selectedVendorPhone = selectedVendor.HomePhone;
                    selectedVendorCity = selectedVendor.City;
                }

                List<Exception> errors;
                var orders = PurchasingService.GetPurchaseOrder(vendorID, out feedback, out errors);

                if (!string.IsNullOrEmpty(feedback))
                {
                    feedBackMessage = feedback;
                }

                if (errors.Any())
                {
                    errorList.AddRange(errors);
                    errorMessage = "Errors encountered while fetching orders.";
                }

                if (orders.Any())
                {
                    PO = orders.First();
                    POnumber = PO.PurchaseOrderNumber;
                    POsubTotal = PO.SubTotal;
                    POgst = PO.GST;
                    POtotal = POsubTotal + POgst;
                    POemployeeID = PO.EmployeeID;
                    POD = PO.PurchaseOrderDetails;
                    POid = PO.PurchaseOrderID;
                }
                else
                {
                    POnumber = 0;
                    POD.Clear();
                    POid = 0;
                }

                LoadInventory();

                await InvokeAsync(StateHasChanged);
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }

        #endregion

        #region Add part to order

        private void AddPartToOrder(ItemView item)
        {
            try
            {
                ClearMessages();
                if (!POD.Any(detail => detail.PartID == item.PartID))
                {
                    POD.Add(new PurchaseOrderDetailView
                    {
                        PartID = item.PartID,
                        Description = item.PartDescription,
                        QOH = item.QOH,
                        ROL = item.ROL,
                        QOO = item.QOO,
                        QTO = item.Buffer,
                        Price = item.Price
                    });

                    LoadInventory();
                    CalculateTotals();
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }


        #endregion

        #region Delete part from order

        private void DeletePartFromOrder(PurchaseOrderDetailView detail)
        {
            try
            {
                POD.Remove(detail);
                LoadInventory();
                CalculateTotals();
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }

        #endregion

        #region New Order

        public async Task NewOrder()
        {
            try
            {
                if (POnumber == 0)
                {
                    if (POD.Any())
                    {
                        CalculateTotals();
                        var uniquePOD = RemoveDuplicates(POD);

                        PurchaseOrderView newOrder = new PurchaseOrderView
                        {
                            VendorID = vendorID,
                            EmployeeID = POemployeeID != 0 ? POemployeeID : 7,
                            GST = POgst,
                            SubTotal = POsubTotal,
                            PurchaseOrderDetails = new List<PurchaseOrderDetailView>(uniquePOD)
                        };

                        List<Exception> errorList;
                        PurchasingService.PurchaseOrder_Save(newOrder, out errorList);
                        if (errorList.Any())
                        {
                            errorDetails.AddRange(errorList.Select(e => e.Message));
                            errorMessage = "Errors occurred while saving the order.";
                            StateHasChanged();
                        }
                        else
                        {
                            feedBackMessage = "Order has been saved successfully.";
                            StateHasChanged();
                        }
                        await FindOrder();
                        StateHasChanged();
                    }
                    else
                    {
                        feedBackMessage = "No order details to save.";
                    }
                }
                else
                {
                    CalculateTotals();
                    var uniquePOD = RemoveDuplicates(POD);

                    PurchaseOrderView existingOrder = new PurchaseOrderView
                    {
                        PurchaseOrderID = POid,
                        PurchaseOrderNumber = POnumber,
                        VendorID = vendorID,
                        EmployeeID = POemployeeID != 0 ? POemployeeID : 7,
                        GST = POgst,
                        SubTotal = POsubTotal,
                        PurchaseOrderDetails = new List<PurchaseOrderDetailView>(uniquePOD)
                    };

                    List<Exception> errorList;
                    PurchasingService.PurchaseOrder_Save(existingOrder, out errorList);
                    if (errorList.Any())
                    {
                        errorDetails.AddRange(errorList.Select(e => e.Message));
                        errorMessage = "Errors occurred while updating the order.";
                        StateHasChanged();
                    }
                    else
                    {
                        feedBackMessage = "Order has been updated successfully.";
                        StateHasChanged();
                    }
                    await FindOrder();
                }

                await InvokeAsync(StateHasChanged);
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }

        #endregion

        #region Place Order

        public async Task PlaceOrder()
        {
            try
            {
                if (POnumber == 0 || POD.Count == 0)
                {
                    errorMessage = "No order to place or order details are missing.";
                    return;
                }

                var uniquePODs = RemoveDuplicates(POD);

                PurchaseOrderView currentOrder = new PurchaseOrderView
                {
                    PurchaseOrderID = POid,
                    PurchaseOrderNumber = POnumber,
                    VendorID = vendorID,
                    EmployeeID = POemployeeID,
                    PurchaseOrderDetails = uniquePODs
                };

                List<Exception> errorList;
                PurchasingService.PurchaseOrder_Place(currentOrder, out errorList);
                if (errorList.Any())
                {
                    errorDetails.AddRange(errorList.Select(e => e.Message));
                    errorMessage = "Errors occurred while placing the order.";
                    StateHasChanged();
                }
                else
                {
                    feedBackMessage = "Order has been placed successfully.";
                    StateHasChanged();
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }

            await InvokeAsync(StateHasChanged);
        }

        #endregion

        #region Delete order

        public async Task DeleteOrder(int orderId)
        {
            try
            {
                PurchaseOrderView orderToDelete = new PurchaseOrderView
                {
                    PurchaseOrderID = orderId,
                    VendorID = vendorID,
                    EmployeeID = POemployeeID
                };

                List<Exception> errorList;
                PurchasingService.PurchaseOrder_Delete(orderToDelete, out errorList);
                if (errorList.Any())
                {
                    errorDetails.AddRange(errorList.Select(e => e.Message));
                    errorMessage = "Errors occurred while updating the order.";
                    StateHasChanged();
                }
                else
                {
                    feedBackMessage = "Order has been deleted successfully.";
                    StateHasChanged();
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }

            await InvokeAsync(StateHasChanged);
        }


        #endregion

    }
}
