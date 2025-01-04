<Query Kind="Program">
  <Connection>
    <ID>cfcfa69a-662d-4b4b-aced-a336cf6e0b12</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>(local)</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
</Query>

#load "..\ViewModels\CustomerView.cs"

using Servicing.ViewModels;

void Main()
{
	Service_GetCustomer("smith").Dump();
}

public List<CustomerView> Service_GetCustomer(string lastName) 
{
	if (string.IsNullOrWhiteSpace(lastName))
		throw new ArgumentNullException("No name provided");
	
	List<CustomerView> customers = Customers
		.Where(c => c.LastName == lastName)
		.Select(c => new CustomerView
		{
			CustomerID = c.CustomerID,
			Name = $"{c.FirstName} {c.LastName}",
			Address = c.Address,
			Phone = c.ContactPhone
		}).ToList();
		
	if (customers.Count == 0)
		throw new ArgumentNullException("No customer found");
		
	return customers;
}