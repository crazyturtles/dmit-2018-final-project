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

#load "..\ViewModels\PurchaseOrderView.cs"


void Main()
{
	try
	{
		
		GetOutstandingOrder().Dump();
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

	public List<PurchaseOrderView> GetOutstandingOrder()
{
	return PurchaseOrders
	//Retrieve only Purchase Orders that are not closed
	.Where ( x => x.Closed != true && x.OrderDate != null)
	.Select (x => new PurchaseOrderView 
	{
		PurchaseOrderID = x.PurchaseOrderID, 
		PurchaseOrderNumber = x.PurchaseOrderNumber,
		OrderDate = x.OrderDate, 
		VendorName = x.Vendor.VendorName, 
		VendorPhone = x.Vendor.Phone	
	})
	.ToList();
}




