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
		int vendorID = 1;
		GetInventory(vendorID).Dump();
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

public List<ItemView> GetInventory(int vendorID)
{
	if (vendorID < 1)
	{
		throw new ArgumentNullException("VendorID is invalid");
	}

	return Parts
					.Where(x => x.VendorID == vendorID)
					.Select(x => new ItemView
					{
						PartID = x.PartID,
						PartDescription = x.Description,
						QOH = x.QuantityOnHand,
						ROL = x.ReorderLevel,
						QOO = x.QuantityOnOrder,
						Buffer = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder),
						Price = x.PurchasePrice
					}).ToList();
}
