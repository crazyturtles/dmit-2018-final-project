<Query Kind="Program">
  <Connection>
    <ID>3cf72ad2-e9ee-4e15-ab66-a90114604b37</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>localhost</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
  <Namespace>Receiving</Namespace>
</Query>

#load "..\ViewModels\ReceivingDetailView.cs"

void Main()
{
	try
	{
		int purchaseOrderID = 456;
		Receiving_FetchOrderDetails(purchaseOrderID).Dump();
	}
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
}
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
// You can define other methods, fields, classes and namespaces here
public List<ReceivingDetailView> Receiving_FetchOrderDetails (int purchaseOrderID)
{
	if (purchaseOrderID <= 0)
	{
		throw new ArgumentNullException("The purchase Order ID is invalid");
	}
	//Create new list of receiving details based on the purchase order details and current parts information:
	//QtyOnOrder
	//QtyOutstanding (QtyOnOrder -(All Receiving QtyReceive) 
	
	
	return ReceiveOrderDetails
	.Where(x => x.PurchaseOrderDetail.PurchaseOrderID == purchaseOrderID) 
	.Select(x => new ReceivingDetailView
	{
		PurchaseOrderDetailID = x.PurchaseOrderDetailID,
		PartID = x.PurchaseOrderDetail.PartID,
		Description = x.PurchaseOrderDetail.Part.Description,
		QtyOnOrder = x.PurchaseOrderDetail.Quantity,
		QtyOutstanding = x.PurchaseOrderDetail.Quantity - x.QuantityReceived,
		QtyReceive = x.QuantityReceived,

		
		QtyReturn = ReturnedOrderDetails
			.Where(x => x.PurchaseOrderDetail.PurchaseOrderID == purchaseOrderID)
			.Select(x => x.Quantity)
			.FirstOrDefault()
		,
		Reason = ReturnedOrderDetails
			.Where(x => x.PurchaseOrderDetail.PurchaseOrderID == purchaseOrderID)
			.Select(x => x.Reason).FirstOrDefault()	
	})
	.ToList();
}