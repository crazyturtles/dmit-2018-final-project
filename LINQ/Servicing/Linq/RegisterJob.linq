<Query Kind="Program">
  <Connection>
    <ID>ad3ff70c-7fe2-498e-ae14-93ad47188b2f</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Server>localhost</Server>
    <Database>eBike_DMIT2018</Database>
    <DisplayName>eBike_DMIT2018-Entity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

#load "..\ViewModels\JobView.cs"
#load "..\ViewModels\JobDetailView.cs"

using Servicing.ViewModels;

void Main()
{
	#region Driver

	try
	{
		JobView job = new()
		{
			EmployeeID = 1,
			ShopRate = 50.75m,
			VehicleIdentificationNumber = "111GT678Y9991",
			TaxAmount = 12.35m,
			SubTotal = 250.00m
		};

		List<JobDetailView> jobDetails = new()
		{
			new()
			{
				Description = "fuel system maintenance",
				JobHour = 35.5m,
				Comment = "Completed ahead of schedule",
				CouponID = 67,
				DiscountPercent = 10
			},
			new()
			{
				Description = "oil change",
				JobHour = 20.75m,
				Comment = "Client requested multiple revisions",
				CouponID = 66,
				DiscountPercent = 15
			},
			new()
			{
				Description = "Spring tune up",
				JobHour = 45.25m,
				Comment = "",
				CouponID = 68,
				DiscountPercent = 20
			}
		};

		RegisterJob(job, jobDetails);
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
	catch (ArgumentException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}

	#endregion
}

private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

public void RegisterJob(JobView job, List<JobDetailView> jobDetails) 
{
	List<Exception> errorList = new();
	
	if (jobDetails != null && jobDetails.Count == 0)
		errorList.Add(new ArgumentNullException("No job details."));
		
	foreach (JobDetailView jobDetail in jobDetails)
	{
		if (string.IsNullOrEmpty(jobDetail.Description))
			errorList.Add(new Exception("Service description is needed."));
		if (jobDetail.JobHour == 0)
			errorList.Add(new Exception($"Hours for job detail {jobDetail.Description} must have a positive value."));
	}
	
	if (errorList.Any())
		throw new AggregateException(errorList);
	else
	{
		Jobs jobRecord = new()
		{
			EmployeeID = job.EmployeeID,
			ShopRate = job.ShopRate,
			VehicleIdentification = job.VehicleIdentificationNumber,
			JobDateIn = DateTime.Now
		};
		
		Jobs.Add(jobRecord);
		
		SaveChanges();

		foreach (JobDetailView jobDetail in jobDetails)
		{
			JobDetails.Add(new()
			{
				JobID = jobRecord.JobID,
				Description = jobDetail.Description,
				JobHours = jobDetail.JobHour,
				Comments = jobDetail.Comment,
				CouponID = jobDetail.CouponID,
				StatusCode = "I"
			});
		}
		
		SaveChanges();
	}
}