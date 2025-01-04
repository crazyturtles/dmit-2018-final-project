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

#load "..\ViewModels\SaleRefundView.cs"

void Main()
{
	SaleRefund_GetSaleRefund(1203).Dump();
}
public SaleRefundView SaleRefund_GetSaleRefund(int saleId)
{
	if(saleId == 0)
	{
		throw new ArgumentNullException("Please provide a valid saleid");
	}
	SaleRefundView sales = 
	Sales
	.Where(x => x.SaleID == saleId)
	.Select(x => new SaleRefundView
	{
		SaleID = x.SaleID,
		EmployeeID = x.EmployeeID,
		TaxAmount = x.TaxAmount,
		SubTotal = x.SubTotal,
		DiscountPercent = x.Coupon.CouponDiscount
	}).FirstOrDefault();
	
	return sales;
}
// You can define other methods, fields, classes and namespaces here