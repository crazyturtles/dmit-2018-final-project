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
</Query>

void Main()
{
	Sale_GetCoupon("Save10").Dump();
}
public int Sale_GetCoupon(string coupons)
{
	return Coupons
	.FirstOrDefault(x => x.CouponIDValue == coupons)?.CouponDiscount ?? 0;
}

// You can define other methods, fields, classes and namespaces here