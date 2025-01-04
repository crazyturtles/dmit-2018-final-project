using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBikeLibrary.ViewModels
{
    public class SaleDetailView
    {
        public int PartID { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Total { get; set; }
    }
}
