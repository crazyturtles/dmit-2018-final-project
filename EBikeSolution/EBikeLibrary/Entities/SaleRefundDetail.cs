﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace EBikeLibrary.Entities;

public partial class SaleRefundDetail
{
    public int SaleRefundDetailId { get; set; }

    public int SaleRefundId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    public decimal SellingPrice { get; set; }

    public string Reason { get; set; }

    public virtual Part Part { get; set; }

    public virtual SaleRefund SaleRefund { get; set; }
}