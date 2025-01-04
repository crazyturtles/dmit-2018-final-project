<Query Kind="Program">
  <Connection>
    <ID>0f817e2e-2ba5-4ade-90b5-aaf7c0fe54ae</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
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
  <Namespace>Purchasing</Namespace>
</Query>

#load ".\ViewModels\*.cs"

void Main()
{
	try
	{
		int vendorID = 5;
		GetPurchaseOrder(vendorID);
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

// You can define other methods, fields, classes and namespaces here

private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

public List<PurchaseOrderView> GetPurchaseOrder(int vendorID)
{
	if (vendorID < 1)
	{
		throw new ArgumentNullException("VendorID provided is invalid.");
	}

	bool vendorExists = 	Vendors
							.Where(x => x.VendorID == vendorID).Any();

	bool orderDateExists = 	PurchaseOrders
							.Where(x => x.VendorID == vendorID && x.OrderDate == null).Any();
						
	bool orderOpen = 		PurchaseOrders
							.Where(x => x.VendorID == vendorID && x.OrderDate == null && x.Closed == false).Any();
	
	if (vendorExists)
	{
		if (orderDateExists)
		{
			if (orderOpen)
			{
				return PurchaseOrders
					.Where(x => x.VendorID == vendorID && x.OrderDate == null)
					.Select(x => new PurchaseOrderView
					{
						PurchaseOrderID = x.PurchaseOrderID,
						PurchaseOrderNumber = x.PurchaseOrderNumber,
						VendorID = x.VendorID,
						SubTotal = x.SubTotal,
						GST = x.TaxAmount,
						EmployeeID = x.EmployeeID,
						PurchaseOrderDetails = x.PurchaseOrderDetails
						.Select(p => new PurchaseOrderDetailView
						{
							PurchaseOrderDetailID = p.PurchaseOrderDetailID,
							PartID = p.PartID,
							Description = p.Part.Description,
							QOH = p.Part.QuantityOnHand,
							ROL = p.Part.ReorderLevel,
							QOO = p.Part.QuantityOnOrder,
							QTO = p.Quantity,
							Price = p.Part.PurchasePrice
						}).ToList()
					}).ToList().Dump();
			}
			else { throw new Exception("Order is closed."); }
		}
		else
		{
			var suggestParts = Parts
			.Where(x => x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder) > 0 && x.VendorID == vendorID)
			.Select(x => new PurchaseOrderDetailView
			{
				PartID = x.PartID,
				Description = x.Description,
				QOH = x.QuantityOnHand,
				ROL = x.ReorderLevel,
				QOO = x.QuantityOnOrder,
				QTO = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder),
				Price = x.PurchasePrice
			}).ToList();
			
			Console.WriteLine($"No active order exists. A suggested purchase order is provided");
			
			List<PurchaseOrderView> emptyPO = new List<PurchaseOrderView>
			{
				new PurchaseOrderView
				{
					VendorID = vendorID,
					SubTotal = suggestParts.Sum(x => x.Price * x.QTO),
					GST = suggestParts.Sum(x => x.Price * x.QTO) * (decimal)0.05,
					PurchaseOrderDetails = suggestParts
				}
			}.Dump();

			Console.WriteLine($"Vendor {vendorID} parts.");
			
			Parts
				.Where(x => x.VendorID == vendorID)
				.Dump();
				
			return emptyPO;
		}
	}
	else { throw new Exception("Vendor does not exist."); }
}