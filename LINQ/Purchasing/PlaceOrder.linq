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

		//POview = PurchaseOrder_Update();
		POview = PurchaseOrder_New();

		PurchaseOrder_Place(POview);
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
			PurchaseOrderID = 		x.PurchaseOrderID,
			PurchaseOrderNumber = 	x.PurchaseOrderNumber,
			OrderDate = 			x.OrderDate,
			VendorID = 				x.VendorID,
			TaxAmount = 			x.TaxAmount,
			SubTotal = 				x.SubTotal,
			EmployeeID = 			x.EmployeeID,
			PurchaseOrderDetails = 	x.PurchaseOrderDetails
										.Select(p => new
										{
											PartID = 		p.PartID,
											Description = 	p.Part.Description,
											QOH = 			p.Part.QuantityOnHand,
											ROL = 			p.Part.ReorderLevel,
											QOO = 			p.Part.QuantityOnOrder,
											QTO = 			p.Quantity,
											Price = 		p.PurchasePrice
										})
		})
		.OrderByDescending(x => x.PurchaseOrderID)
		.ToList().Dump();
}

public int GetOrderNumber()
{
	int OrderNumberGenerate = PurchaseOrders
								.Select(x => x.PurchaseOrderNumber).Max();
	OrderNumberGenerate++;
	return OrderNumberGenerate;
}

public void PurchaseOrder_Place(PurchaseOrderView purchaseOrderView)
{
	List<Exception> errorlist = new List<Exception>();
	PurchaseOrders PO = new PurchaseOrders();

	PurchaseOrders POClosedStatus = PurchaseOrders
										.Where(x =>
											x.VendorID == purchaseOrderView.VendorID
											&& x.PurchaseOrderID == purchaseOrderView.PurchaseOrderID).FirstOrDefault();

	if (POClosedStatus != null && POClosedStatus.OrderDate != null)
	{
		throw new ArgumentNullException("Order is closed and cannot be edited or placed");
	}

	if (purchaseOrderView == null || purchaseOrderView.PurchaseOrderDetails.Count == 0)
	{
		throw new ArgumentNullException("There is no order details");
	}

	bool employeeValidation = Employees
								.Where(x => x.EmployeeID == purchaseOrderView.EmployeeID).Any();

	if (!employeeValidation)
	{
		errorlist.Add(new Exception($"EmployeeID {purchaseOrderView.EmployeeID} does not exist."));
	}

	PurchaseOrders isOrderActive = PurchaseOrders
									.Where(x => x.VendorID == purchaseOrderView.VendorID
											&& x.OrderDate == null).FirstOrDefault();

	//if (isOrderActive == null)
	//{
	//	// The query returned null, which means no active orders were found that match the conditions.
	//	Console.WriteLine("No active orders found for the specified conditions.");
	//}
	//else
	//{
	//	// The query found an active order.
	//	Console.WriteLine("An active order was found.");
	//}

	foreach (var POdetail in purchaseOrderView.PurchaseOrderDetails)
	{
		if (POdetail.PartID == 0)
		{
			throw new ArgumentNullException($"Part of order Num {purchaseOrderView.PurchaseOrderNumber} cannot be zero.");
		}

		if (POdetail.QTO <= 0)
		{
			errorlist.Add(new Exception($"Quantity for PO item {POdetail.Description} must have a positive non zero value."));
		}

		if (POdetail.Price <= 0)
		{
			errorlist.Add(new Exception($"Price for PO item {POdetail.Description} must have a positive non zero value."));
		}

		Parts part = Parts
					.Where(x => x.PartID == POdetail.PartID
							&& x.VendorID == purchaseOrderView.VendorID).FirstOrDefault();

		if (part == null)
		{
			errorlist.Add(new Exception($"PO item {POdetail.Description} does not exist with the vendor."));
		}

		if (errorlist.Count == 0)
		{
			if (isOrderActive != null)
			{
				PurchaseOrderDetails activeOrderDetail = isOrderActive.PurchaseOrderDetails
														.Where(x => x.PartID == POdetail.PartID).FirstOrDefault();

				if (activeOrderDetail != null)
				{
					activeOrderDetail.Quantity = POdetail.QTO;
					activeOrderDetail.PurchasePrice = POdetail.Price;
				}
				else
				{
					Parts newPOpart = Parts
									.Where(x => x.PartID == POdetail.PartID
											&& x.VendorID == purchaseOrderView.VendorID).FirstOrDefault();
					PurchaseOrderDetails newPOdetail = new PurchaseOrderDetails();

					if (newPOpart != null)
					{
						newPOdetail.PartID = newPOpart.PartID;
						newPOdetail.Part = newPOpart;
						newPOdetail.Quantity = POdetail.QTO;
						newPOdetail.PurchasePrice = POdetail.Price;
						isOrderActive.PurchaseOrderDetails.Add(newPOdetail);
					}
					else
					{
						throw new ArgumentNullException($"Part {POdetail.Description} does not exist.");
					}
				}
			}
			else
			{
				PurchaseOrderDetails activeOrderDetail = new PurchaseOrderDetails();
				activeOrderDetail.Part = Parts
											.Where(x => x.PartID == POdetail.PartID).FirstOrDefault();
				activeOrderDetail.PartID = POdetail.PartID;
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
			PO.VendorID = purchaseOrderView.VendorID;
			PO.SubTotal = PO.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
			PO.TaxAmount = PO.SubTotal * (decimal)0.05;
			PO.EmployeeID = purchaseOrderView.EmployeeID;
			PO.OrderDate = DateTime.Now;
			PurchaseOrders.Add(PO);
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
		throw new AggregateException("Errors: ", errorlist.OrderBy(x => x.Message).ToList());
	}
	else
	{
		SaveChanges();
	}
}

public PurchaseOrderView PurchaseOrder_Update()
{
	PurchaseOrderView PO = new PurchaseOrderView();

	PO.PurchaseOrderID = 570;
	PO.PurchaseOrderNumber = 134;
	PO.VendorID = 5;
	PO.SubTotal = PurchaseOrders
									.Where(x => x.PurchaseOrderID == PO.PurchaseOrderID)
									.Select(x => x.SubTotal).FirstOrDefault();
	PO.GST = PurchaseOrders
									.Where(x => x.PurchaseOrderID == PO.PurchaseOrderID)
									.Select(x => x.TaxAmount).FirstOrDefault();
	PO.EmployeeID = 7;
	PO.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

	PO.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = 101,
		Description = Parts
							.Where(x => x.PartID == 101)
							.Select(x => x.Description)
							.FirstOrDefault(),
		QOH = Parts
							.Where(x => x.PartID == 101)
							.Select(x => x.QuantityOnHand)
							.FirstOrDefault(),
		ROL = Parts
							.Where(x => x.PartID == 101)
							.Select(x => x.ReorderLevel)
							.FirstOrDefault(),
		QOO = Parts
							.Where(x => x.PartID == 101)
							.Select(x => x.QuantityOnOrder)
							.FirstOrDefault(),
		QTO = 10,
		Price = (decimal)25
	});

	PO.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = 108,
		Description = Parts
							.Where(x => x.PartID == 108)
							.Select(x => x.Description)
							.FirstOrDefault(),
		QOH = Parts
							.Where(x => x.PartID == 108)
							.Select(x => x.QuantityOnHand)
							.FirstOrDefault(),
		ROL = Parts
							.Where(x => x.PartID == 108)
							.Select(x => x.ReorderLevel)
							.FirstOrDefault(),
		QOO = Parts
							.Where(x => x.PartID == 108)
							.Select(x => x.QuantityOnOrder)
							.FirstOrDefault(),
		QTO = 3,
		Price = (decimal)270
	});

	PO.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = 115,
		Description = Parts
							.Where(x => x.PartID == 115)
							.Select(x => x.Description)
							.FirstOrDefault(),
		QOH = Parts
							.Where(x => x.PartID == 115)
							.Select(x => x.QuantityOnHand)
							.FirstOrDefault(),
		ROL = Parts
							.Where(x => x.PartID == 115)
							.Select(x => x.ReorderLevel)
							.FirstOrDefault(),
		QOO = Parts
							.Where(x => x.PartID == 115)
							.Select(x => x.QuantityOnOrder)
							.FirstOrDefault(),
		QTO = 24,
		Price = (decimal)50
	});

	PO.Dump();
	return PO;
}

public PurchaseOrderView PurchaseOrder_New()
{
	PurchaseOrderView PO = new PurchaseOrderView();

	PO.VendorID = 2;
	PO.EmployeeID = 5;
	PO.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

	PO.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = 102,
		Description = Parts
							.Where(x => x.PartID == 102)
							.Select(x => x.Description)
							.FirstOrDefault(),
		QOH = Parts
							.Where(x => x.PartID == 102)
							.Select(x => x.QuantityOnHand)
							.FirstOrDefault(),
		ROL = Parts
							.Where(x => x.PartID == 102)
							.Select(x => x.ReorderLevel)
							.FirstOrDefault(),
		QOO = Parts
							.Where(x => x.PartID == 102)
							.Select(x => x.QuantityOnOrder)
							.FirstOrDefault(),
		QTO = 35,
		Price = (decimal)25
	});

	PO.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = 103,
		Description = Parts
							.Where(x => x.PartID == 103)
							.Select(x => x.Description)
							.FirstOrDefault(),
		QOH = Parts
							.Where(x => x.PartID == 103)
							.Select(x => x.QuantityOnHand)
							.FirstOrDefault(),
		ROL = Parts
							.Where(x => x.PartID == 103)
							.Select(x => x.ReorderLevel)
							.FirstOrDefault(),
		QOO = Parts
							.Where(x => x.PartID == 103)
							.Select(x => x.QuantityOnOrder)
							.FirstOrDefault(),
		QTO = 1,
		Price = (decimal)50
	});

	PO.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = 104,
		Description = Parts
							.Where(x => x.PartID == 104)
							.Select(x => x.Description)
							.FirstOrDefault(),
		QOH = Parts
							.Where(x => x.PartID == 104)
							.Select(x => x.QuantityOnHand)
							.FirstOrDefault(),
		QOO = Parts
							.Where(x => x.PartID == 104)
							.Select(x => x.QuantityOnOrder)
							.FirstOrDefault(),
		ROL = Parts
							.Where(x => x.PartID == 104)
							.Select(x => x.ReorderLevel)
							.FirstOrDefault(),
		QTO = 1,
		Price = (decimal)50
	});

	PO.Dump();
	return PO;
}