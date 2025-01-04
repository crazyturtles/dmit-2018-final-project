﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace EBikeLibrary.Entities;

public partial class Vendor
{
    public int VendorId { get; set; }

    public string VendorName { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string ProvinceId { get; set; }

    public string PostalCode { get; set; }

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}