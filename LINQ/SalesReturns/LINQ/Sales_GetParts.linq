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

#load "..\ViewModels\PartView.cs"

void Main()
{
	Sale_GetParts(103).Dump();;
}
public List<PartView> Sale_GetParts(int categoryid)
{
	if(categoryid == 0)
	{
		throw new ArgumentNullException("Please provide a category ID");
	}
	List<PartView> parts = 
	Parts
	.Where(x => x.Category.CategoryID == categoryid || !x.Discontinued)
	.Select(x =>
		new  PartView
		{
			PartID = x.PartID,
			Description = x.Description,
			SellingPrice = x.SellingPrice,
		}).ToList();
		
	return parts;
}
// You can define other methods, fields, classes and namespaces here