<Query Kind="Program" />


// You can define other methods, fields, classes and namespaces here
public class SaleRefundView
{
	public int SaleId { get; set; }
	public int EmployeeId { get; set; }
	public decimal TaxAmount { get; set; }
	public decimal SubTotal { get; set; }
	public int DiscountPercent { get; set; }
}

public class SaleRefundDetailView
{
	public int PartID { get; set; }
	public string Description { get; set; }
	public int OriginalQuantity { get; set; }
	public decimal SellingPrice { get; set; }
	public int ReturnQuantity { get; set; }
	public bool Refundable { get; set; }
	public int Quantity { get; set; }
	public string Reason { get; set; }
}