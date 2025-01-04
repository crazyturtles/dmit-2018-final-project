using EBikeLibrary.DAL;
using EBikeLibrary.Entities;
using EBikeLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBikeLibrary.BLL
{
    public class ReceivingService
    {
        #region Fields
        private readonly eBike_DMIT2018Context _ebike_dmit2018Context;
        #endregion
        //Constructor for the ReceivingService class
        internal ReceivingService(eBike_DMIT2018Context ebike_dmit2018Context)
        {
            _ebike_dmit2018Context = ebike_dmit2018Context;
        }

        private class ReceiveOrderDetails
        {
            public int PurchaseOrderDetailID { get; set; }
            public int QuantityReceived { get; set; }
        }

        private class ReturnedOrderDetails
        {
            public int PurchaseOrderDetailID { get; set; }
            public int Quantity { get; set; }
            public string Reason { get; set; }
        }

        private class ReceiveOrders
        {
            public int PurchaseOrderID { get; set; }
            public DateTime ReceiveDate { get; set; }
        }
        public List<PurchaseOrderView> GetOutstandingOrder()
        {
            return _ebike_dmit2018Context.PurchaseOrders
                .Where(x => x.Closed != true && x.OrderDate != null)
                .Select(x => new PurchaseOrderView
                {
                    PurchaseOrderID = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrderNumber,
                    OrderDate = x.OrderDate,
                    VendorName = x.Vendor.VendorName,
                    VendorPhone = x.Vendor.Phone
                })
                .ToList();
        }
        public List<ReceivingDetailView> Receiving_FetchOrderDetails(int PurchaseOrderID)
        {
            return _ebike_dmit2018Context.ReceiveOrderDetails
                .Where(x => x.ReceiveOrder.PurchaseOrderId == PurchaseOrderID)
                .Select(x => new ReceivingDetailView
                {
                    PurchaseOrderDetailID = x.PurchaseOrderDetailId,
                    PartID = x.PurchaseOrderDetail.Part.PartId,
                    Description = x.PurchaseOrderDetail.Part.Description,
                    QtyOnOrder = x.PurchaseOrderDetail.Part.QuantityOnOrder,
                    QtyOutstanding = x.PurchaseOrderDetail.Part.QuantityOnOrder - x.QuantityReceived,
                    QtyReceive = x.QuantityReceived,
                    QtyReturn = _ebike_dmit2018Context.ReturnedOrderDetails
                                        .Where(r => r.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderID)
                                        .Select(r => r.Quantity).FirstOrDefault(),
                    Reason = _ebike_dmit2018Context.ReturnedOrderDetails
                                        .Where(r => r.PurchaseOrderDetail.PurchaseOrderId == PurchaseOrderID)
                                        .Select(r => r.Reason).FirstOrDefault()
                })
                .ToList();

        }
        
        public void Receiving_ForceClose(int PurchaseOrderID, List<ReceivingDetailView> receivingDetails, List<UnorderedReturnItemView> unorderedReturnItems, string reason)
        {
            // Implement logic to handle force closing
            try
            {
                //  Update the order status to indicate that it has been force closed
                var order = _ebike_dmit2018Context.PurchaseOrders.Select(o => o.PurchaseOrderId == PurchaseOrderID);
                if (order != null)
                {
                    // order.Status = "Force Closed";
                    // Update the order details and unordered items accordingly
                    // Save changes to the database
                    _ebike_dmit2018Context.Remove(PurchaseOrderID);
                }
                else
                {
                    throw new InvalidOperationException("Order not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error force closing order.", ex);
            }
        }
        public void Receiving_Receive(int PurchaseOrderID, 
            List<ReceivingDetailView> receiveDetails,
            List<UnorderedReturnItemView> unorderedReturnItems, string reason)
        {
            List<Exception> errorList = new List<Exception>();

            foreach (var detail in receiveDetails)
            {// is received quantity greater than the outstanding order 
             //if yes, then 	
                if (detail.QtyReceive > detail.QtyOutstanding)
                {
                    errorList.Add(new Exception($"Quantity for receiving item {detail.QtyReceive} is greater than the outstanding order"));
                }
                //Received Qty is a positive value 
                //no 
                if (detail.QtyReceive <= 0)
                {
                    errorList.Add(new Exception($"Quantity for receiving item {detail.QtyReceive} must have a positive value"));
                }
                // Check if the return quantity is a positive value
                if (detail.QtyReturn < 0)
                {
                    errorList.Add(new Exception($"Return for receiving item {detail.QtyReturn} must have a positive value"));
                }
                //yes 
                //does it have a reason 
                //no
                if (detail.QtyReturn > 0 && string.IsNullOrEmpty(detail.Reason))
                {
                    errorList.Add(new Exception($"There must be a reason for the return for receiving item {detail.QtyReturn}"));
                }
            }
            //foreach unordered return item 
            foreach (var item in unorderedReturnItems)
            {   //is there an item description 
                //no 
                if (string.IsNullOrEmpty(item.Description))
                {
                    errorList.Add(new Exception($"There must be an item description for unordered items"));
                }
                //Unorder Qty is a positive value 
                //no
                if (item.Quantity <= 0)
                {
                    errorList.Add(new Exception($"Unorder item {item.Quantity} must have a positive value"));
                }
            }
            //check for errors
            //YES 
            //throw the list of all collected exception 
            if (errorList.Any())
            {
                throw new AggregateException("Errors occurred while processing your request.", errorList);
            }
            else
            {
                //save all work to the database 
                //Create ReceivingOrder 	
                var receivingOrder = new ReceiveOrders
                {
                    PurchaseOrderID = PurchaseOrderID,
                    ReceiveDate = DateTime.Today,
                };
                //Create ReceiveOrderdetail for each receivingDetails that has a QtyReceive greater than 0 
                //QtyReceive greater than 0 
                var receiveOrderDetails = receiveDetails
                    .Where(detail => detail.QtyReceive > 0)
                    .Select(detail => new ReceiveOrderDetails
                    {
                        //PurchaseOrderDetailID
                        //QuantityReceived
                        PurchaseOrderDetailID = detail.PurchaseOrderDetailID,
                        QuantityReceived = detail.QtyReceive
                    });
                //update Parts 
                foreach (var detail in receiveDetails)
                {
                    if (detail.QtyReceive > 0)
                    {
                        var part = _ebike_dmit2018Context.Parts.FirstOrDefault(p => p.PartId == detail.PartID);
                        if (part != null)
                        {
                            part.QuantityOnHand += detail.QtyReceive;
                            // You may need to save changes to the context if required
                            _ebike_dmit2018Context.SaveChanges();
                        }
                    }
                }
                // Create ReturnOrderDetail for each receivingDetails that has a return greater than > 0 
                var returnOrderDetailsFromReceiving = receiveDetails
                        .Where(detail => detail.QtyReturn > 0)
                        .Select(detail => new ReturnedOrderDetails
                        {
                            PurchaseOrderDetailID = detail.PurchaseOrderDetailID,
                            Quantity = detail.QtyReturn,
                            Reason = detail.Reason
                        })
                        .ToList();

                // Create ReturnOrderDetail for unorderedReturnItems with Qty greater than 0
                var returnOrderDetailsFromUnordered = unorderedReturnItems
                    .Where(detail => detail.Quantity > 0)
                    .Select(detail => new UnorderedPurchaseItemCart
                    {
                        Description = detail.Description,
                        Quantity = detail.Quantity,
                        VendorPartNumber = detail.VSN

                    })
                    .ToList();
                //Check if all items have been received 
                if (errorList.Any())
                {
                    throw new AggregateException("Errors occurred while processing your request.", errorList);
                }
                    _ebike_dmit2018Context.SaveChanges();
            }
           


        }

       

       




    }
}

