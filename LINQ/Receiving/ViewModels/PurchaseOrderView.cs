namespace Receiving
{
	// ---------Receiving View Models------------
	public class PurchaseOrderView
	{
		public int PurchaseOrderID { get; set; }
		public int PurchaseOrderNumber { get; set; }
		public DateTime? OrderDate { get; set; }
		public string VendorName { get; set; }
		public string VendorPhone { get; set; }
	}
}