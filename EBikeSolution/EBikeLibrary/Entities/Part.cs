﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace EBikeLibrary.Entities;

public partial class Part
{
    public int PartId { get; set; }

    public string Description { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public int QuantityOnHand { get; set; }

    public int ReorderLevel { get; set; }

    public int QuantityOnOrder { get; set; }

    public int CategoryId { get; set; }

    public string Refundable { get; set; }

    public bool Discontinued { get; set; }

    public int VendorId { get; set; }

    public virtual Category Category { get; set; }

    public virtual ICollection<JobDetailPart> JobDetailParts { get; set; } = new List<JobDetailPart>();

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    public virtual ICollection<SaleRefundDetail> SaleRefundDetails { get; set; } = new List<SaleRefundDetail>();

    public virtual Vendor Vendor { get; set; }
}