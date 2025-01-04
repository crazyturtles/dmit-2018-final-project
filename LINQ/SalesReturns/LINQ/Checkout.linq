<Query Kind="Expression">
  <Connection>
    <ID>b9a82935-b821-4800-8588-080dacd615d3</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>MSI\SQLEXPRESS</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
</Query>

#load "..\ViewModels\SaleView"
#load "..\ViewModels\SaleDetailView"

public int Checkout(SaleView sale, List<SaleDetailView> saleDetails)
{
	List<Exception> errorList = new List<Exception>();
	//String paymentType = new String();
	//salesdetails count is greater than 0
	if(salesDetails.Count() !> 0)
	//	- no
	//	- throw NullArgumentexception (no sales details)
	{
		throw new ArgumentNullException("No sales details");
	}
	//foreach sale detial in sale details
	foreach (var sales in saleDetails)
	{
		//quantity is greater than 0
		if (sales.Quantity() > 0)
		//	- errorList.Add(new Exception($"Quantity for sale item () must have a positive value"));
		{
			errorList.Add(new Exception($"Quantity for sale item () must have a positive value"));
		}
	}
	//check for errors
	if(errorList.Count > 0)
	//	yes 
	//	throw the list of all collected exceptions
	{
		throw new AggregateException(errorList);
	}
	//	no
	else
	{
	//	save all work to the database
	//		create record in sales
		saleRecord = new Sales();
	//			-set SaleDate to today	
	//			-update all fields 
	//			-ensure that subtotal and tax are discounted 
	//			based on coupon amount
		saleRecord.SaleDate = DateTime.Now();
		saleRecord.EmployeeID = sale.EmployeeID;
		saleRecord.TaxAmount = sale.TaxAmount - ((sale.DiscountPercent/100)*sale.Taxamount);
		saleRecord.SubTotal = sale.Subtotal - ((sale.DiscountPercent/100)*sale.Subtotal);
		saleRecord.CouponID = sale.CouponID;
		saleRecord.PaymentType = sale.PaymentType;
		Sales.Add(saleRecord);
	//		create records in SaleDetails
		foreach(var sales in SaleDetails)
	//		update Parts QuantityOnHand 
	//		(QuantityOnHand - saleDetailQuantity)
		{
			saleDetailRecord = new SaleDetails();
			saleDetailRecord.PartID = sales.PartID;
			saleDetailRecord.Quantity = sales.Quantity;
			saleDetailRecord.SellingPrice = sales.SellingPrice;
			
			var part = Parts.Where(p => p.PartID == sales.PartID).Select(p => p);
			part.QuantityOnHand = (part.QuantityOnHand - sales.Quantity);
			Parts.Update(part);
			SaleDetails.Add(saleDetailRecord);
		}
		SaveChanges();
	}
	//	return SaleID
	return saleRecord.SaleID;
	//	Disable checkout button
	
}

