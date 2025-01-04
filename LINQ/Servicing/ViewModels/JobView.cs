namespace Servicing.ViewModels;

public class JobView
{
    public int EmployeeID { get; set; }
    public decimal ShopRate { get; set; }
    public string VehicleIdentificationNumber { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal SubTotal { get; set; }
}