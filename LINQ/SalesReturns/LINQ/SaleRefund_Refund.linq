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

#load "..\ViewModels\SaleRefundView"
#load "..\ViewModels\SaleRefundDetailView"

public SaleRefund_Refund(SaleRefundView saleRefund, List<SaleRefundDetailView> saleRefundDetails)
{

	//foreach saleRefundDetail in saleRefundDetails
	foreach (var saleRefundDetail in saleRefundDetails)
	{
		//quantity is less than zero
		//	errorList.Add(new Exception ($"Quantity for item () is less than zero(0)"));
		if (saleRefundDetail.ReturnQuantity < 0)
		{
			errorList.Add(new Exception($"Quantity for item {saleRefundDetail.Description} is less than zero(0)"));
		}
		//quantity is greater than zero and no reason given
		//	errorList.Add(new Exception($"There is no reason given for item ()"));
		if (saleRefundDetail.Quantity > 0 && string.IsNullOrEmpty(saleRefundDetail.Reason))
		{
			errorList.Add(new Exception($"There is no reason given for item {saleRefundDetail.Description}"));
		}
		//Quantity return is larger than(original quantity - return quantity)
		//	errorList.Add(new Exception($"Quantity for item is greater than the original quantity");
		if (saleRefundDetail.OriginalQuantity - saleRefundDetail.ReturnQuantity < 0)
		{
			errorList.Add(new Exception($"Quantity for {saleRefundDetail.Description} is greater than the original quantity");
		}
		//validate that item can be refundable (Parts.Refundable)
		//	no
		//		errorList.Add(new Exception($"item is not refundable"));
		if (!(Parts.Where(p => p.PartID == saleRefundDetail.PartID).Select(p => p.Refundable)))
		{
			errorList.Add(new Exception($"{saleRefundDetail.Description} is not refundable"));
		}
	}
		//Check for errors
		//	yes
		//		throw the list of all collected exceptions
		if (errorList.Count > 0)
		{
			throw new AggregateException(errorList);
		}
		//	no
		else
		{
			//		save all work to the database
			//			SaleRefund
			//				set SaleRefundDate to today
			//				ensure that subtotal and tax are discounted based on coupon amount
			refund = new SaleRefunds();
			refund.SaleRefundDate = DateTime.Now();
			//			SaleRefundDetails
			refund.SaleID = saleRefund.SaleID;
			refund.EmployeeID = saleRefund.EmployeeID;
			refund.TaxAmount = saleRefund.TaxAmount - ((saleRefund.DiscountPercent/100)*saleRefund.TaxAmount);
			refund.SubTotal = saleRefund.SubTotal - ((saleRefund.DiscountPercent/100)* saleRefund.SubTotal);
			SaleRefunds.Update(refund);
			foreach(var saleRefundDetail in saleRefundDetails)
			{
				returnDetail = new SaleRefundDetails();
				returnDetail.PartID = saleRefundDetail.PartID;
				returnDetail.Quantity = saleRefundDetail.ReturnQuantity;
				returnDetail.SellingPrice = saleRefundDetail.SellingPrice;
				returnDetail.Reason = saleRefundDetai.Reason;
				SaleRefundDetails.Update(returnDetail);
			//	Parts
			//	Increase QunatityOnHand == QuantityOnHand + SaleRefundDetail.Quantity
				var part = Parts.Where(p => p.PartID == saleRefundDetail.PartID).Select(p => p);
				part.QuantityOnHand = (part.QuantityOnHand + saleRefundDetail.Quantity);
				Parts.Update(part);
			}
		}	
		return refund;
		//Return updated saleRefund
		//disable refund buttonn
	
}