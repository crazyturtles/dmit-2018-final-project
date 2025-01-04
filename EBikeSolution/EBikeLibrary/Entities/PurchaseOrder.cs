﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace EBikeLibrary.Entities;

public partial class PurchaseOrder
{
    public int PurchaseOrderId { get; set; }

    public int PurchaseOrderNumber { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal SubTotal { get; set; }

    public bool Closed { get; set; }

    public string Notes { get; set; }

    public int EmployeeId { get; set; }

    public int VendorId { get; set; }

    public virtual Employee Employee { get; set; }

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual ICollection<ReceiveOrder> ReceiveOrders { get; set; } = new List<ReceiveOrder>();

    public virtual Vendor Vendor { get; set; }
}