using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBikeLibrary.ViewModels
{
    public class UnorderedReturnItemView
    {
        public int CartId { get; set; }
        public string Description { get; set; }
        public string VSN { get; set; }
        public int Quantity { get; set; }
    }
}
