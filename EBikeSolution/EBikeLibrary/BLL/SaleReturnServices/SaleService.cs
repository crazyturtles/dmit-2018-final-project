using EBikeLibrary.DAL;
using EBikeLibrary.Entities;
using EBikeLibrary.ViewModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBikeLibrary.BLL.SaleReturnServices
{
    public class SaleService
    {
        #region fields
        private readonly eBike_DMIT2018Context _eBikeContext;
        #endregion

        internal SaleService(eBike_DMIT2018Context eBikeContext)
        {
            _eBikeContext = eBikeContext;
        }

        #region Lookups
        public List<CategoryView> GetCategories()
        {
            return _eBikeContext.Categories
                .Select(x => new CategoryView
                {
                    CategoryID = x.CategoryId,
                    Description = x.Description
                })
                .ToList();
        }
        public int GetCoupon(string couponcode)
        {
            return _eBikeContext.Coupons
                .FirstOrDefault(x => x.CouponIdvalue == couponcode)?.CouponDiscount ?? 0;
        }
        public List<PartView> GetParts(int categoryid)
        {
            if (categoryid == 0)
            {
                throw new ArgumentNullException("Please provide a cateogry ID");
            }
            List<PartView> parts = _eBikeContext.Parts
                .Where(x => x.Category.CategoryId == categoryid || !x.Discontinued)
                .Select(x =>
                    new PartView
                    {
                        PartID = x.PartId,
                        Description = x.Description,
                        SellingPrice = x.SellingPrice
                    }).ToList();

            return parts;
        }
        #endregion
        #region Command methods
        public SaleView Checkout(SaleView sale, List<SaleDetailView> saleDetails)
        {
            List<Exception> errorList = new List<Exception>();
            if (saleDetails.Count()! > 0)
            {
                throw new ArgumentNullException("No sales details");
            }
            foreach (var sales in saleDetails)
            {
                if (sales.Quantity > 0)
                {
                    errorList.Add(new Exception($"Quantity for sale item () must have a positive value"));
                }
            }
            if (errorList.Count > 0)
            {
                throw new AggregateException(errorList);
            }
            else
            {
                Sale saleRecord = new Sale();
                saleRecord.SaleDate = DateTime.Now;
                saleRecord.EmployeeId = sale.EmployeeID;
                saleRecord.TaxAmount = sale.TaxAmount - sale.DiscountPercent / 100 * sale.TaxAmount;
                saleRecord.SubTotal = sale.SubTotal - sale.DiscountPercent / 100 * sale.SubTotal;
                saleRecord.CouponId = sale.CouponID;
                saleRecord.PaymentType = sale.PaymentType;
                _eBikeContext.Sales.Add(saleRecord);

                foreach (var sales in saleDetails)
                {
                    SaleDetail saleDetailRecord = new SaleDetail();
                    saleDetailRecord.PartId = sales.PartID;
                    saleDetailRecord.Quantity = sales.Quantity;
                    saleDetailRecord.SellingPrice = sales.SellingPrice;
                    Part part = _eBikeContext.Parts.Where(p => p.PartId == sales.PartID).Select(p => p).FirstOrDefault();

                    part.QuantityOnHand = part.QuantityOnHand - sales.Quantity;
                    _eBikeContext.Parts.Update(part);
                    //saleDetails.Add(saleDetailRecord);
                }
                _eBikeContext.SaveChanges();
                return sale;
            }

        }
        #endregion

    }
}
