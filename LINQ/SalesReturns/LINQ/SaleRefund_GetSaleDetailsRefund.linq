<Query Kind="Program">
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
  <Namespace>SalesReturns.ViewModels</Namespace>
</Query>

#load "..\ViewModels\SaleRefundDetailView.cs"

void Main()
{
	SaleRefund_GetSaleDetailsRefund(1203).Dump();
}
public List<SaleRefundDetailView> SaleRefund_GetSaleDetailsRefund(int saleId)
{
	if (saleId == 0)
	{
		throw new ArgumentNullException("Please provide a valid saleid");
	}
	return SaleRefundDetails
	.Where(x => x.SaleRefund.SaleID == saleId)
	.Select(x =>
	new SaleRefundDetailView
	{
		PartID = x.PartID,
		Description = x.Part.Description,
		OriginalQuantity = x.Quantity,
		SellingPrice = x.SellingPrice,
		Refundable = new(),
		Quantity = new(),
		Reason = x.Reason
	}).ToList();
}

// You can define other methods, fields, classes and namespaces here