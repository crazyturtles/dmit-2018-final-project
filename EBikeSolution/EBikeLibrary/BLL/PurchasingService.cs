using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBikeLibrary.DAL;
using EBikeLibrary.ViewModels.Purchasing;
using EBikeLibrary.Entities;
using EBikeLibrary.Paginator;
using System.IO;

namespace EBikeLibrary.BLL
{
    public class PurchasingService
    {
        #region Fields

        private readonly eBike_DMIT2018Context _eBikeContext;

        #endregion

        internal PurchasingService(eBike_DMIT2018Context eBikeContext)
        {
            _eBikeContext = eBikeContext;
        }

        #region GetPurchaseOrder

        public List<PurchaseOrderView> GetPurchaseOrder(int vendorID, out string feedbackMessage, out List<Exception> errorList)
        {
            // Initialize feedback message and error list.
            feedbackMessage = string.Empty;
            errorList = new List<Exception>();

            // Check if the provided vendorID is less than 1, which is invalid.
            if (vendorID < 1)
            {
                errorList.Add(new ArgumentException("VendorID provided is invalid."));
            }

            // Check if there exists any vendor with the given vendorID.
            bool vendorExists = _eBikeContext.Vendors
                                .Any(x => x.VendorId == vendorID);

            // Check if there are any purchase orders that have not been placed.
            bool orderDateExists = _eBikeContext.PurchaseOrders
                                .Any(x => x.VendorId == vendorID && x.OrderDate == null);

            // Check if there are any open purchase orders for this vendor (not closed and not placed).
            bool orderOpen = _eBikeContext.PurchaseOrders
                                .Any(x => x.VendorId == vendorID && x.OrderDate == null && !x.Closed);

            // If a vendor exists:
            if (vendorExists)
            {
                // If there's at least one unplaced purchase order:
                if (orderDateExists)
                {
                    // If there's at least one open purchase order:
                    if (orderOpen)
                    {
                        // Retrieve and return all open purchase orders with their details.
                        return _eBikeContext.PurchaseOrders
                            .Where(x => x.VendorId == vendorID && x.OrderDate == null && !x.Closed)
                            .Select(x => new PurchaseOrderView
                            {
                                PurchaseOrderID = x.PurchaseOrderId,
                                PurchaseOrderNumber = x.PurchaseOrderNumber,
                                VendorID = x.VendorId,
                                SubTotal = x.SubTotal,
                                GST = x.TaxAmount,
                                EmployeeID = x.EmployeeId,
                                PurchaseOrderDetails = x.PurchaseOrderDetails
                                    .Select(p => new PurchaseOrderDetailView
                                    {
                                        PurchaseOrderDetailID = p.PurchaseOrderDetailId,
                                        PartID = p.PartId,
                                        Description = p.Part.Description,
                                        QOH = p.Part.QuantityOnHand,
                                        ROL = p.Part.ReorderLevel,
                                        QOO = p.Part.QuantityOnOrder,
                                        QTO = p.Quantity,
                                        Price = p.Part.PurchasePrice
                                    }).ToList()
                            }).ToList();
                    }
                    else
                    {
                        feedbackMessage = "Order is closed.";
                        return _eBikeContext.PurchaseOrders
                            .Where(x => x.VendorId == vendorID && x.OrderDate == null)
                            .Select(x => new PurchaseOrderView
                            {
                                PurchaseOrderID = x.PurchaseOrderId,
                                PurchaseOrderNumber = x.PurchaseOrderNumber,
                                VendorID = x.VendorId,
                                SubTotal = x.SubTotal,
                                GST = x.TaxAmount,
                                EmployeeID = x.EmployeeId,
                                PurchaseOrderDetails = x.PurchaseOrderDetails
                                    .Select(p => new PurchaseOrderDetailView
                                    {
                                        PurchaseOrderDetailID = p.PurchaseOrderDetailId,
                                        PartID = p.PartId,
                                        Description = p.Part.Description,
                                        QOH = p.Part.QuantityOnHand,
                                        ROL = p.Part.ReorderLevel,
                                        QOO = p.Part.QuantityOnOrder,
                                        QTO = p.Quantity,
                                        Price = p.Part.PurchasePrice
                                    }).ToList()
                            }).ToList();
                    }
                }
                else
                {
                    feedbackMessage = "No active order exists. A suggested purchase order is provided.";
                    var suggestParts = _eBikeContext.Parts
                        .Where(x => x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder) > 0 && x.VendorId == vendorID)
                        .Select(x => new PurchaseOrderDetailView
                        {
                            PartID = x.PartId,
                            Description = x.Description,
                            QOH = x.QuantityOnHand,
                            ROL = x.ReorderLevel,
                            QOO = x.QuantityOnOrder,
                            QTO = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder),
                            Price = x.PurchasePrice
                        }).ToList();

                    return new List<PurchaseOrderView>
            {
                new PurchaseOrderView
                {
                    VendorID = vendorID,
                    SubTotal = suggestParts.Sum(x => x.Price * x.QTO),
                    GST = suggestParts.Sum(x => x.Price * x.QTO) * (decimal)0.05,
                    PurchaseOrderDetails = suggestParts
                }
            };
                }
            }
            else
            {
                errorList.Add(new Exception("Vendor does not exist."));
            }

            return null;
        }


        #endregion

        #region GetInventory

        public List<ItemView> GetInventory(int vendorID)
        {
            if (vendorID < 1)
            {
                throw new ArgumentNullException("VendorID is invalid");
            }

            return _eBikeContext.Parts
                            .Where(x => x.VendorId == vendorID)
                            .Select(x => new ItemView
                            {
                                PartID = x.PartId,
                                PartDescription = x.Description,
                                QOH = x.QuantityOnHand,
                                ROL = x.ReorderLevel,
                                QOO = x.QuantityOnOrder,
                                Buffer = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder),
                                Price = x.PurchasePrice
                            }).ToList();
        }

        #endregion

        #region GetVendors

        public List<VendorView> GetVendors()
        {
            return _eBikeContext.Vendors
                .Select(x => new VendorView
                {
                    VendorID = x.VendorId,
                    VendorName = x.VendorName,
                    HomePhone = x.Phone,
                    City = x.City
                }).ToList();
        }

        #endregion

        #region Place and Save Order

        public int GetOrderNumber()
        {
            int OrderNumberGenerate = _eBikeContext.PurchaseOrders
                                        .Select(x => x.PurchaseOrderNumber).Max();
            OrderNumberGenerate++;
            return OrderNumberGenerate;
        }

        #region PlaceOrder

        public void PurchaseOrder_Place(PurchaseOrderView purchaseOrderView, out List<Exception> errorlist)
        {
            errorlist = new List<Exception>();
            PurchaseOrder PO = new PurchaseOrder();

            PurchaseOrder POClosedStatus = _eBikeContext.PurchaseOrders
                                                .Where(x =>
                                                    x.VendorId == purchaseOrderView.VendorID
                                                    && x.PurchaseOrderId == purchaseOrderView.PurchaseOrderID).FirstOrDefault();

            if (POClosedStatus != null && POClosedStatus.OrderDate != null)
            {
                errorlist.Add(new ArgumentNullException("Order is closed and cannot be edited or placed"));
            }

            if (purchaseOrderView == null || purchaseOrderView.PurchaseOrderDetails.Count == 0)
            {
                errorlist.Add(new ArgumentNullException("There is no order details"));
            }

            bool employeeValidation = _eBikeContext.Employees
                                        .Where(x => x.EmployeeId == purchaseOrderView.EmployeeID).Any();

            if (!employeeValidation)
            {
                errorlist.Add(new Exception($"EmployeeID {purchaseOrderView.EmployeeID} does not exist."));
            }

            PurchaseOrder isOrderActive = _eBikeContext.PurchaseOrders
                                            .Where(x => x.VendorId == purchaseOrderView.VendorID
                                                    && x.OrderDate == null).FirstOrDefault();

            foreach (var POdetail in purchaseOrderView.PurchaseOrderDetails)
            {
                if (POdetail.PartID == 0)
                {
                    errorlist.Add(new ArgumentNullException($"Part of order Num {purchaseOrderView.PurchaseOrderNumber} cannot be zero."));
                }

                if (POdetail.QTO <= 0)
                {
                    errorlist.Add(new Exception($"Quantity for PO item {POdetail.Description} must have a positive non zero value."));
                }

                if (POdetail.Price <= 0)
                {
                    errorlist.Add(new Exception($"Price for PO item {POdetail.Description} must have a positive non zero value."));
                }

                Part part = _eBikeContext.Parts
                            .Where(x => x.PartId == POdetail.PartID
                                    && x.VendorId == purchaseOrderView.VendorID).FirstOrDefault();

                if (part == null)
                {
                    errorlist.Add(new Exception($"PO item {POdetail.Description} does not exist with the vendor."));
                }

                if (errorlist.Count == 0)
                {
                    if (isOrderActive != null)
                    {
                        PurchaseOrderDetail activeOrderDetail = isOrderActive.PurchaseOrderDetails
                                                                .Where(x => x.PartId == POdetail.PartID).FirstOrDefault();

                        if (activeOrderDetail != null)
                        {
                            activeOrderDetail.Quantity = POdetail.QTO;
                            activeOrderDetail.PurchasePrice = POdetail.Price;
                        }
                        else
                        {
                            Part newPOpart = _eBikeContext.Parts
                                            .Where(x => x.PartId == POdetail.PartID
                                                    && x.VendorId == purchaseOrderView.VendorID).FirstOrDefault();
                            PurchaseOrderDetail newPOdetail = new PurchaseOrderDetail();

                            if (newPOpart != null)
                            {
                                newPOdetail.PartId = newPOpart.PartId;
                                newPOdetail.Part = newPOpart;
                                newPOdetail.Quantity = POdetail.QTO;
                                newPOdetail.PurchasePrice = POdetail.Price;
                                isOrderActive.PurchaseOrderDetails.Add(newPOdetail);
                            }
                            else
                            {
                                errorlist.Add(new ArgumentNullException($"Part {POdetail.Description} does not exist."));
                            }
                        }
                    }
                    else
                    {
                        PurchaseOrderDetail activeOrderDetail = new PurchaseOrderDetail();
                        activeOrderDetail.Part = _eBikeContext.Parts
                                                    .Where(x => x.PartId == POdetail.PartID).FirstOrDefault();
                        activeOrderDetail.PartId = POdetail.PartID;
                        activeOrderDetail.PurchasePrice = POdetail.Price;
                        activeOrderDetail.Quantity = POdetail.QTO;
                        PO.PurchaseOrderDetails.Add(activeOrderDetail);
                    }
                    part.QuantityOnOrder = part.QuantityOnOrder + POdetail.QTO;
                }
            }

            if (errorlist.Count == 0)
            {
                if (isOrderActive == null)
                {
                    PO.PurchaseOrderNumber = GetOrderNumber();
                    PO.VendorId = purchaseOrderView.VendorID;
                    PO.SubTotal = PO.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
                    PO.TaxAmount = PO.SubTotal * (decimal)0.05;
                    PO.EmployeeId = purchaseOrderView.EmployeeID;
                    PO.OrderDate = DateTime.Now;
                    _eBikeContext.PurchaseOrders.Add(PO);
                }
                else
                {
                    isOrderActive.SubTotal = isOrderActive.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
                    isOrderActive.TaxAmount = isOrderActive.SubTotal * (decimal)0.05;
                    isOrderActive.OrderDate = DateTime.Now;
                }
            }

            if (errorlist.Count > 0)
            {
                errorlist.Add(new AggregateException("Errors: ", errorlist.OrderBy(x => x.Message).ToList()));
            }
            else
            {
                _eBikeContext.SaveChanges();
            }
        }

        #endregion

        #region SaveOrder

        public void PurchaseOrder_Save(PurchaseOrderView purchaseOrderView, out List<Exception> errorlist)
        {
            // List to store any exceptions that might occur during the save process
            errorlist = new List<Exception>();

            // Creating a new instance of PurchaseOrder to hold the details
            PurchaseOrder PO = new PurchaseOrder();

            // Checking if the order is already closed in the database
            PurchaseOrder POClosedStatus = _eBikeContext.PurchaseOrders
                                                .FirstOrDefault(x =>
                                                    x.VendorId == purchaseOrderView.VendorID
                                                    && x.PurchaseOrderNumber == purchaseOrderView.PurchaseOrderNumber);

            // If the order is found and it's closed, throw an exception
            if (POClosedStatus != null && POClosedStatus.OrderDate != null)
            {
                errorlist.Add(new Exception("Order is closed and cannot be edited or placed"));
            }

            // Validate if the input view model is null or contains no details
            if (purchaseOrderView == null || purchaseOrderView.PurchaseOrderDetails.Count == 0)
            {
                errorlist.Add(new Exception("There is no order details"));
            }

            // Check if the employee ID is valid
            bool employeeValidation = _eBikeContext.Employees
                                        .Any(x => x.EmployeeId == purchaseOrderView.EmployeeID);

            if (!employeeValidation)
            {
                errorlist.Add(new Exception($"EmployeeID {purchaseOrderView.EmployeeID} does not exist."));
            }

            // Check if there's an active order for the vendor that hasn't been closed
            PurchaseOrder isOrderActive = _eBikeContext.PurchaseOrders
                                            .FirstOrDefault(x => x.VendorId == purchaseOrderView.VendorID
                                                    && x.OrderDate == null);

            // Loop through each detail in the purchase order details
            foreach (var POdetail in purchaseOrderView.PurchaseOrderDetails)
            {
                if (POdetail.PartID == 0)
                {
                    errorlist.Add(new ArgumentNullException($"Part of order Num {purchaseOrderView.PurchaseOrderNumber} cannot be zero."));
                }

                if (POdetail.QTO <= 0)
                {
                    errorlist.Add(new Exception($"Quantity for PO item {POdetail.Description} must have a positive non zero value."));
                }

                if (POdetail.Price <= 0)
                {
                    errorlist.Add(new Exception($"Price for PO item {POdetail.Description} must have a positive non zero value."));
                }

                // Retrieve the part from the database to ensure it exists and matches the vendor ID
                Part part = _eBikeContext.Parts
                            .FirstOrDefault(x => x.PartId == POdetail.PartID
                                    && x.VendorId == purchaseOrderView.VendorID);

                if (part == null)
                {
                    errorlist.Add(new Exception($"PO item {POdetail.Description} does not exist with the vendor."));
                }

                // If no errors have accumulated, process the order details
                if (errorlist.Count == 0)
                {
                    // If there's an active order, update it
                    if (isOrderActive != null)
                    {
                        PurchaseOrderDetail activeOrderDetail = isOrderActive.PurchaseOrderDetails
                                                                .FirstOrDefault(x => x.PartId == POdetail.PartID);

                        if (activeOrderDetail != null)
                        {
                            activeOrderDetail.Quantity = POdetail.QTO;
                            activeOrderDetail.PurchasePrice = POdetail.Price;
                        }
                        else
                        {
                            // If the part detail doesn't exist in the active order, create a new detail
                            PurchaseOrderDetail newPOdetail = new PurchaseOrderDetail
                            {
                                PartId = part.PartId,
                                Part = part,
                                Quantity = POdetail.QTO,
                                PurchasePrice = POdetail.Price
                            };
                            isOrderActive.PurchaseOrderDetails.Add(newPOdetail);
                        }
                    }
                    else
                    {
                        // If there's no active order, create a new order detail
                        PurchaseOrderDetail activeOrderDetail = new PurchaseOrderDetail
                        {
                            Part = part,
                            PartId = POdetail.PartID,
                            PurchasePrice = POdetail.Price,
                            Quantity = POdetail.QTO
                        };
                        PO.PurchaseOrderDetails.Add(activeOrderDetail);
                    }
                }
            }

            // If there were no errors, either add the new order to the database or update the existing order
            if (errorlist.Count == 0)
            {
                if (isOrderActive == null)
                {
                    // Set the new order number, vendor ID, and employee ID, then add the order to the database
                    PO.PurchaseOrderNumber = GetOrderNumber();
                    PO.VendorId = purchaseOrderView.VendorID;
                    PO.SubTotal = PO.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
                    PO.TaxAmount = PO.SubTotal * (decimal)0.05;
                    PO.EmployeeId = purchaseOrderView.EmployeeID;
                    PO.OrderDate = null; // Setting the order date as null for a new order
                    _eBikeContext.PurchaseOrders.Add(PO);
                }
                else
                {
                    // Update the subtotal and tax amount for the active order
                    isOrderActive.SubTotal = isOrderActive.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
                    isOrderActive.TaxAmount = isOrderActive.SubTotal * (decimal)0.05;
                }
            }

            // If errors were found, throw an AggregateException containing all error messages
            if (errorlist.Count > 0)
            {
                errorlist.Add(new AggregateException("Errors: ", errorlist.OrderBy(x => x.Message).ToList()));
            }
            else
            {
                // Save changes to the database and return the new order number
                _eBikeContext.SaveChanges();
            }
        }

        #endregion

        #endregion

        #region Delete Order

        public void PurchaseOrder_Delete(PurchaseOrderView purchaseOrderView, out List<Exception> errorlist)
        {
            errorlist = new List<Exception>();
            PurchaseOrder PO = new PurchaseOrder();

            PurchaseOrder POClosedStatus = _eBikeContext.PurchaseOrders
                                                    .Where(x =>
                                                        x.VendorId == purchaseOrderView.VendorID
                                                        && x.PurchaseOrderId == purchaseOrderView.PurchaseOrderID).FirstOrDefault();

            if (POClosedStatus != null && POClosedStatus.OrderDate != null)
            {
                errorlist.Add(new ArgumentNullException("Order is closed and cannot be deleted."));
            }

            if (purchaseOrderView == null)
            {
                errorlist.Add(new ArgumentNullException("There is no order details"));
            }

            bool employeeValidation = _eBikeContext.Employees
                                        .Where(x => x.EmployeeId == purchaseOrderView.EmployeeID).Any();

            if (!employeeValidation)
            {
                errorlist.Add(new Exception($"EmployeeID {purchaseOrderView.EmployeeID} does not exist."));
            }

            if (!errorlist.Any())
            {
                var detailsOfOrderToBeRemoved = _eBikeContext.PurchaseOrderDetails
                                             .Where(detail => detail.PurchaseOrderId == purchaseOrderView.PurchaseOrderID)
                                             .ToList();

                var orderToBeRemoved = _eBikeContext.PurchaseOrders.FirstOrDefault(order => order.PurchaseOrderId == purchaseOrderView.PurchaseOrderID);

                foreach (var detail in detailsOfOrderToBeRemoved)
                {
                    _eBikeContext.PurchaseOrderDetails.Remove(detail);
                }

                if (orderToBeRemoved != null)
                {
                    _eBikeContext.PurchaseOrders.Remove(orderToBeRemoved);
                }
            }
            else
            {
                errorlist.Add(new AggregateException("Unable to delete the order due to the following issues", errorlist.OrderBy(error => error.Message).ToList()));
            }

            if (errorlist.Count == 0)
            {
                _eBikeContext.SaveChanges();
            }
        }

        #endregion

    }
}
