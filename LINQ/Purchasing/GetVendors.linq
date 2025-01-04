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
	try { GetVendors().Dump(); }
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

public List<VendorView> GetVendors()
{
	return Vendors
		.Select(x => new VendorView
		{
			VendorID = x.VendorID,
			VendorName = x.VendorName,
			HomePhone = x.Phone,
			City = x.City
		}).ToList();
}