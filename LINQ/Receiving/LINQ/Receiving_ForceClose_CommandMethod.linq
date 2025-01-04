<Query Kind="Program">
  <Connection>
    <ID>0f817e2e-2ba5-4ade-90b5-aaf7c0fe54ae</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
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
</Query>

#load "../ViewModels/ReceivingDetailView.cs"
#load "../ViewModels/UnorderedReturnItemView.cs"
using Receiving;
void Main()
{
	#region Driver 
		try
		{	
			ReceivingDetailView receivingDetailView = new ReceivingDetailView();
			int PurchaseOrderID = 1234;
			int employeeId = 123456;		
		}
		#region  catch all exceptions
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
		#endregion
	#endregion
}

public Exception GetInnerException(System.Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

// You can define other methods, fields, classes and namespaces here
//An order can be forcibly closed, such as when the supplier is unable to fulfill the order. A reason must be provided for this action
public void Receiving_ForceClose(int PurchaseOrderID, int employeeId, List<ReceivingDetailView> receivingDetails, List<UnorderedReturnItemView> unorderedReturnItems)
{
	List<Exception> errorList = new List<Exception>();
	//Save rules as Receiving_receive 
	//On Save 
	//Update Close 
	//Update Note 
		// Check if the purchase order exists and if it is open
		var purchaseOrder = PurchaseOrders.FirstOrDefault(po => po.PurchaseOrderID == PurchaseOrderID && !po.Closed);

		if (purchaseOrder == null)
		{
			throw new Exception($"Purchase order with ID {PurchaseOrderID} does not exist or is already closed.");
		}

		// Update the status of the purchase order to closed
		purchaseOrder.Closed = true;
		purchaseOrder.EmployeeID = employeeId;
		purchaseOrder.OrderDate = DateTime.Now;

		// Save changes to the database
		SaveChanges();
}



