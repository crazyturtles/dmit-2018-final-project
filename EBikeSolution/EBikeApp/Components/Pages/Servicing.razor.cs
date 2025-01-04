using EBikeLibrary.BLL;
using EBikeLibrary.ViewModels.Servicing;
using Microsoft.AspNetCore.Components;

namespace EBikeApp.Components.Pages;

public partial class Servicing
{
    const decimal SHOP_RATE = 65;

    #region Fields

    private List<CustomerView> customers = new();

    private string customerSearchValue = string.Empty;

    private string? feedbackMessage;

    private string? errorMessage;

    private bool hasFeedback 
        => !string.IsNullOrWhiteSpace(feedbackMessage);

    private bool hasError 
        => !string.IsNullOrWhiteSpace(errorMessage);

    private List<string> errorDetails = new();

    private CustomerView? selectedCustomer = null;

    private List<CustomerVehicleView> selectedCustomersVehicles = new();

    private CustomerVehicleView? selectedVehicle = null;

    private List<string> standardServices = new();

    private JobDetailView currentJobDetail = new();

    private List<JobDetailView> currentJobDetails = new();

    private string couponValue = string.Empty;

    private int discountPercentage = 0;

    private decimal TotalHours 
        => currentJobDetails!.Sum(jd => jd.JobHour);

    private decimal SubTotal
        => currentJobDetails!.Sum(jd => jd.JobHour - (jd.JobHour * jd.DiscountPercent / 100m)) * SHOP_RATE;

    private decimal TaxAmount
        => SubTotal * (decimal)0.05;

    #endregion

    #region Properties

    [Inject]
    protected ServicingService ServicingService { get; set; }

    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        try
        {
            errorDetails.Clear();

            errorMessage = string.Empty;

            feedbackMessage = string.Empty;

            standardServices = ServicingService.Service_GetStandardService();

            await InvokeAsync(StateHasChanged);
        }
        catch (ArgumentNullException ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
        catch (ArgumentException ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
        catch (Exception ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
    }

    public static Exception GetInnerException(Exception ex)
    {
        while (ex.InnerException != null)
            ex = ex.InnerException;
        return ex;
    }

    private void CustomerLookup()
    {
        try
        {
            List<Exception> errors = new();

            Clear();

            if (string.IsNullOrWhiteSpace(customerSearchValue))
                errors.Add(new Exception("Customer search value is empty."));

            if (errors.Count != 0)
                throw new AggregateException("Couldn't search for customers: ", errors);

            customers = ServicingService.Service_GetCustomer(customerSearchValue);
            feedbackMessage = $"Successfully search for customers. {customers.Count} result(s)";
            customerSearchValue = string.Empty;
        }
        catch (ArgumentNullException ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
        catch (ArgumentException ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
        catch (AggregateException ex)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = $"{errorMessage}{Environment.NewLine}";

            errorMessage = $"{errorMessage} Unable to search customers";

            foreach (var error in ex.InnerExceptions)
                errorDetails.Add(error.Message);
        }
    }

    private void SelectCustomer(int customerId)
    {
        try
        {
            selectedCustomer = new();
            selectedCustomersVehicles = new();
            selectedVehicle = null;

            selectedCustomer = customers.First(c => c.CustomerID == customerId);

            selectedCustomersVehicles = ServicingService.GetCustomerVehicle(selectedCustomer.CustomerID);
        }
        catch (Exception ex)
        {
            errorMessage = $"Could not select customer: {GetInnerException(ex).Message}";
        }
    }


    private void OnVehicleSelectionChanged(ChangeEventArgs e)
    {
        try
        {
            selectedVehicle = null;

            if (e.Value is string val && val != "0" && selectedCustomersVehicles.Count != 0)
            {
                selectedVehicle = selectedCustomersVehicles.First(v => v.VehicleIdentificationNumber == val);
            }
        }
        catch (Exception ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
    }

    private void RegisterJob()
    {
        try
        {
            if (selectedVehicle == null)
                throw new Exception("Selected vehicle cannot be null.");

            JobView job = new()
            {
                EmployeeID = 1,
                ShopRate = SHOP_RATE,
                VehicleIdentificationNumber = selectedVehicle!.VehicleIdentificationNumber,
                SubTotal = SubTotal,
                TaxAmount = TaxAmount
            };

            ServicingService.RegisterJob(job, currentJobDetails);

            Clear();

            feedbackMessage = "Job registered!";
        }
        catch (ArgumentNullException ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
        catch (ArgumentException ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
        catch (AggregateException ex)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = $"{errorMessage}{Environment.NewLine}";

            errorMessage = $"{errorMessage} Unable to search customers";

            foreach (var error in ex.InnerExceptions)
                errorDetails.Add(error.Message);
        }
        catch (Exception ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
    }

    private void ValidateCoupon()
    {
        try
        {
            feedbackMessage = string.Empty;
            errorMessage = string.Empty;
            errorDetails = new();

            discountPercentage = ServicingService.GetCoupon(couponValue);

            if (discountPercentage == 0)
                feedbackMessage = "Coupon Invalid";

            couponValue = string.Empty;
        }
        catch (Exception ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
    }

    private void AddDetailToJob()
    {
        try
        {
            feedbackMessage = string.Empty;
            errorMessage = string.Empty;
            errorDetails = new();
            
            if (discountPercentage > 0)
                currentJobDetail.DiscountPercent = discountPercentage;

            List<Exception> errors = new();

            if (currentJobDetail.JobHour <= 0)
                errors.Add(new Exception("Job Hour must be greater than 0."));

            if (string.IsNullOrWhiteSpace(currentJobDetail.Description))
                errors.Add(new Exception("Standard Service cannot be empty."));


            if (errors.Count != 0)
                throw new AggregateException("Couldn't search for customers: ", errors);

            currentJobDetails.Add(currentJobDetail);
            ResetCurrentJobDetail();
        }
        catch (ArgumentNullException ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
        catch (ArgumentException ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
        catch (AggregateException ex)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = $"{errorMessage}{Environment.NewLine}";

            errorMessage = $"{errorMessage} Unable to search customers";

            foreach (var error in ex.InnerExceptions)
                errorDetails.Add(error.Message);
        }
    }

    private void RemoveJobDetail(JobDetailView jobDetail)
    {
        try
        {
            currentJobDetails.Remove(jobDetail);
        }
        catch (Exception ex)
        {
            errorMessage = GetInnerException(ex).Message;
        }
    }

    private void ResetCoupon()
        => discountPercentage = 0;

    private void ResetCurrentJobDetail()
    {
        feedbackMessage = string.Empty;
        errorMessage = string.Empty;
        errorDetails = new();
        couponValue = string.Empty;
        currentJobDetail = new();
        discountPercentage = 0;
    }

    private void Clear()
    {
        feedbackMessage = string.Empty;
        errorMessage = string.Empty;
        errorDetails = new();
        customers = new();
        selectedCustomer = null;
        selectedCustomersVehicles = new();
        selectedVehicle = null;
        couponValue = string.Empty;
        discountPercentage = 0;
        currentJobDetail = new();
        currentJobDetails = new();
    }
}
