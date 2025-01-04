﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBikeLibrary.ViewModels.Purchasing
{
    public class ItemView
    {
        public int PartID { get; set; }
        public string PartDescription { get; set; }
        public int QOH { get; set; }
        public int ROL { get; set; }
        public int QOO { get; set; }
        public int Buffer { get; set; }
        public decimal Price { get; set; }
    }
}