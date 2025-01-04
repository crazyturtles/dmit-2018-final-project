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
  <Namespace>Servicing.ViewModels</Namespace>
</Query>

#load "..\ViewModels\CustomerVehicleView.cs"

using Servicing.ViewModels;

void Main()
{
	GetCustomerVehicle(1).Dump();
}

public List<CustomerVehicleView> GetCustomerVehicle(int customerID) 
{	
	List<CustomerVehicleView> customerVehicles = CustomerVehicles
		.Where(cv => cv.Customer.CustomerID == customerID)
		.Select(cv => new CustomerVehicleView
		{
			VehicleIdentificationNumber = cv.VehicleIdentification,
			MakeModel = $"{cv.Make} {cv.Model}"
		})
		.ToList();
	
	if (customerVehicles == null || customerVehicles.Count == 0)
		throw new ArgumentNullException("No vehicle provided.");
	
	return customerVehicles;
}