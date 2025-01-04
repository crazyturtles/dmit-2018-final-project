namespace Servicing.ViewModels;

public class JobDetailView
{
    public string Description { get; set; }
    public decimal JobHour { get; set; }
    public string Comment { get; set; }
    public int CouponID { get; set; }
    public int DiscountPercent { get; set; }
}