namespace Receiving
{
	
	public class ReceivingDetailView
	{
		public int PurchaseOrderDetailID { get; set; }
		public int PartID { get; set; }
		public string Description { get; set; }
		public int QtyOnOrder { get; set; }
		public int QtyOutstanding { get; set; }
		public int QtyReceive { get; set; }
		public int QtyReturn { get; set; }
		public string Reason { get; set; }
	}

}