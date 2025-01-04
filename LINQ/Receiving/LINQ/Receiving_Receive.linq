<Query Kind="Program">
  <Connection>
    <ID>0f817e2e-2ba5-4ade-90b5-aaf7c0fe54ae</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Server>.</Server>
    <Database>eBike_DMIT2018</Database>
    <DisplayName>eBike_DMIT2018-Entity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

#load "../ViewModels/ReceivingDetailView.cs"
#load "../ViewModels/UnorderedReturnItemView.cs"
using Receiving;
void Main()
{
	#region Driver 
		try
			{
				ReceivingDetailView receivingDetailView = new ReceivingDetailView();
				
				string purchaseOrderID = "123456"; 
				
				string employeeId = "18"; 
				
				string reason = "broken";
			}
			#region  catch all exceptions
				catch (AggregateException ex)
				{
					foreach (var error in ex.InnerExceptions)
					{
						error.Message.Dump();
					}
				}
				catch (ArgumentNullException ex)
				{
					GetInnerException(ex).Message.Dump();
				}
				catch (Exception ex)
				{
					GetInnerException(ex).Message.Dump();
				}
			#endregion
	#endregion
}

// You can define other methods, fields, classes and namespaces here
private Exception GetInnerException(System.Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

public void Receiving_Receive (int PurchaseOrderID, int employedId, 
	List<ReceivingDetailView> receiveDetails, 
	List<UnorderedReturnItemView> unorderedReturnItems, string reason )
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
		{	//is there an item description 
				//no 
				if (string.IsNullOrEmpty(item.Description)){
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
				var receivingOrder = new ReceivingOrder
				{
					PurchaseOrderID = PurchaseOrderID,
					ReceiveDate = DateTime.Today,
					EmployedId = employedId
				};
				//Create ReceiveOrderdetail for each receivingDetails that has a QtyReceive greater than 0 
				//QtyReceive greater than 0 
				var receiveOrderDetails = receiveDetails
					.Where(detail => detail.QtyReceive > 0)
					.Select(detail => new ReceiveOrderDetail
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
								var part = Parts.FirstOrDefault(p => p.PartID == detail.PartID);
								if (part != null)
								{
									part.QuantityOnHand += detail.QtyReceive;
									
								}
							}
						}
				// Create ReturnOrderDetail for each receivingDetails that has a return greater than > 0 
				var returnOrderDetailsFromReceiving = receiveDetails
						.Where(detail => detail.QtyReturn > 0)
						.Select(detail => new ReturnOrderDetail
						{
						PurchaseOrderDetailID = detail.PurchaseOrderDetailID,
						QuantityReturned = detail.QtyReturn,
						ReturnReason = detail.Reason
						})
						.ToList();

				// Create ReturnOrderDetail for unorderedReturnItems with Qty greater than 0
				var returnOrderDetailsFromUnordered = unorderedReturnItems
					.Where(detail => detail.Quantity > 0)
					.Select(detail => new ReturnOrderDetail
					{
						Description = detail.Description,
						QuantityReturned = detail.Quantity,
						// Set other properties of ReturnOrderDetail as needed
					})
					.ToList();
		//Check if all items have been received 
		bool allItemsReceived = receiveDetails.All(detail => detail.QtyReceive >= detail.QtyOutstanding);
		SaveChanges();
		}
	}
		
}












