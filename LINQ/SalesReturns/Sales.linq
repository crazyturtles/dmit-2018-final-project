<Query Kind="Program" />


// You can define other methods, fields, classes and namespaces here

public class CategoryView
{
	public int CategoryID { get; set; }
	public string Description { get; set; }
}

public class PartView
{
	public int PartID { get; set; }
	public string Description { get; set; }
	public decimal SellingPrice  { get; set; }
}

public class SaleView
{
	public int EmployeeID { get; set; }
	public decimal TaxAmount { get; set; }
	public decimal SubTotal { get; set; }
	public int CouponId { get; set; }
	public int DiscountPercent { get; set; }
	public string PaymentType { get; set; }
}

public class SaleDetailView
{
	public int PartID { get; set; }
	public string Description { get; set; }
	public int Quantity { get; set; }
	public decimal SellingPrice { get; set; }
	public decimal Total { get; set; }
}