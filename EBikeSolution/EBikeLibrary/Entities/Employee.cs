﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace EBikeLibrary.Entities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string SocialInsuranceNumber { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string Province { get; set; }

    public string PostalCode { get; set; }

    public string ContactPhone { get; set; }

    public bool Textable { get; set; }

    public string EmailAddress { get; set; }

    public int PositionId { get; set; }

    public virtual ICollection<JobDetail> JobDetails { get; set; } = new List<JobDetail>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual Position Position { get; set; }

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<SaleRefund> SaleRefunds { get; set; } = new List<SaleRefund>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}