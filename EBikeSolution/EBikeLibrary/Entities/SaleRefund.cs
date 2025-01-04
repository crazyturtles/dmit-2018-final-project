﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace EBikeLibrary.Entities;

public partial class SaleRefund
{
    public int SaleRefundId { get; set; }

    public DateTime SaleRefundDate { get; set; }

    public int SaleId { get; set; }

    public int EmployeeId { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal SubTotal { get; set; }

    public virtual Employee Employee { get; set; }

    public virtual Sale Sale { get; set; }

    public virtual ICollection<SaleRefundDetail> SaleRefundDetails { get; set; } = new List<SaleRefundDetail>();
}