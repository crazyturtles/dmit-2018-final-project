using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBikeLibrary.ViewModels
{
    public class ReceivingDetailView
    {
        public int PurchaseOrderID { get; set; }
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
