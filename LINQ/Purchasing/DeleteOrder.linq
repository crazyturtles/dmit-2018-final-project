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
		PurchaseOrderView POview = null;
		ShowPO();

		POview = GenerateSampleOrderData();

		PurchaseOrder_Delete(POview);
		ShowPO();
	}

	//  catch all exceptions
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

	catch (ArgumentException ex)
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

public void ShowPO()
{
	PurchaseOrders
		.Take(100)
		.Select(x => new
		{
			PurchaseOrderID = x.PurchaseOrderID,
			PurchaseOrderNumber = x.PurchaseOrderNumber,
			OrderDate = x.OrderDate,
			VendorID = x.VendorID,
			TaxAmount = x.TaxAmount,
			SubTotal = x.SubTotal,
			EmployeeID = x.EmployeeID,
			PurchaseOrderDetails = x.PurchaseOrderDetails
										.Select(p => new
										{
											PartID = p.PartID,
											Description = p.Part.Description,
											QOH = p.Part.QuantityOnHand,
											ROL = p.Part.ReorderLevel,
											QOO = p.Part.QuantityOnOrder,
											QTO = p.Quantity,
											Price = p.PurchasePrice
										})
		})
		.OrderByDescending(x => x.PurchaseOrderID)
		.ToList().Dump();
}

public void PurchaseOrder_Delete(PurchaseOrderView purchaseOrderView)
{
	List<Exception> errorlist = new List<Exception>();
	PurchaseOrders PO = new PurchaseOrders();

	PurchaseOrders POClosedStatus = PurchaseOrders
											.Where(x =>
												x.VendorID == purchaseOrderView.VendorID
												&& x.PurchaseOrderID == purchaseOrderView.PurchaseOrderID).FirstOrDefault();

	if (POClosedStatus != null && POClosedStatus.OrderDate != null)
	{
		throw new ArgumentNullException("Order is closed and cannot be deleted.");
	}

	if (purchaseOrderView == null)
	{
		throw new ArgumentNullException("There is no order details");
	}

	bool employeeValidation = Employees
								.Where(x => x.EmployeeID == purchaseOrderView.EmployeeID).Any();

	if (!employeeValidation)
	{
		errorlist.Add(new Exception($"EmployeeID {purchaseOrderView.EmployeeID} does not exist."));
	}

	
	if (!errorlist.Any()) 
	{
		var detailsOfOrderToBeRemoved = PurchaseOrderDetails
                                     .Where(detail => detail.PurchaseOrderID == purchaseOrderView.PurchaseOrderID)
                                     .ToList();

		var orderToBeRemoved = PurchaseOrders.FirstOrDefault(order => order.PurchaseOrderID == purchaseOrderView.PurchaseOrderID);

		foreach (var detail in detailsOfOrderToBeRemoved)
		{
			PurchaseOrderDetails.Remove(detail);
		}

		if (orderToBeRemoved != null)
		{
			PurchaseOrders.Remove(orderToBeRemoved);
		}
	}
	else
	{
		throw new AggregateException("Unable to delete the order due to the following issues", errorlist.OrderBy(error => error.Message).ToList());
	}
	
	if (errorlist.Count == 0)
	{
		SaveChanges();
	}
}

public PurchaseOrderView GenerateSampleOrderData()
{
	var sampleOrder = new PurchaseOrderView
	{
		PurchaseOrderID = 570,
		VendorID = 5,
		EmployeeID = 7
	};

	sampleOrder.Dump();
	return sampleOrder;
}
