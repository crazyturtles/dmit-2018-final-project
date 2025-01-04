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

#load "..\ViewModels\CategoryView.cs"

void Main() 
{
	Sale_GetCategories().Dump();
}

public List<CategoryView> Sale_GetCategories()
{
	return Categories
	.Select(x =>
	new CategoryView
	{
		CategoryID = x.CategoryID,
		Description = x.Description
	}
	).ToList();
}

	
