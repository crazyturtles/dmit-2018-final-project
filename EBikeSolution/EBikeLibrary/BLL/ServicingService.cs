using EBikeLibrary.DAL;
using EBikeLibrary.Entities;
using EBikeLibrary.ViewModels.Servicing;

namespace EBikeLibrary.BLL;

public class ServicingService
{
    #region Fields

    private readonly eBike_DMIT2018Context _eBikeContext;

    #endregion

    internal ServicingService(eBike_DMIT2018Context eBikeContext)
    {
        _eBikeContext = eBikeContext;
    }

    #region Service Methods

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

        if (errorList.Count != 0)
            throw new AggregateException(errorList);
        else
        {
            Job jobRecord = new()
            {
                EmployeeId = job.EmployeeID,
                ShopRate = job.ShopRate,
                VehicleIdentification = job.VehicleIdentificationNumber,
                JobDateIn = DateTime.Now
            };

            foreach (JobDetailView jobDetail in jobDetails)
            {
                jobRecord.JobDetails.Add(new()
                {
                    JobId = jobRecord.JobId,
                    Description = jobDetail.Description,
                    JobHours = jobDetail.JobHour,
                    Comments = jobDetail.Comment,
                    CouponId = jobDetail.CouponID == 0 ? null : jobDetail.CouponID,
                    StatusCode = "I"
                });
            }

            _eBikeContext.Jobs.Add(jobRecord);

            _eBikeContext.SaveChanges();
        }
    }

    public List<CustomerVehicleView> GetCustomerVehicle(int customerID)
    {
        List<CustomerVehicleView> customerVehicles = _eBikeContext.CustomerVehicles
            .Where(cv => cv.Customer.CustomerId == customerID)
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

    public List<CustomerView> Service_GetCustomer(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentNullException("No name provided");

        List<CustomerView> customers = _eBikeContext.Customers
            .Where(c => c.LastName.Contains(lastName))
            .Select(c => new CustomerView
            {
                CustomerID = c.CustomerId,
                Name = $"{c.FirstName} {c.LastName}",
                Address = c.Address,
                Phone = c.ContactPhone
            }).ToList();

        if (customers.Count == 0)
            throw new ArgumentNullException("No customer found");

        return customers;
    }


    public int GetCoupon(string coupon) =>
        _eBikeContext.Coupons.FirstOrDefault(c => c.CouponIdvalue == coupon)?.CouponDiscount ?? 0;


    public List<string> Service_GetStandardService()
        => _eBikeContext.StandardJobs.Select(sj => sj.Description).ToList();

    #endregion
}
