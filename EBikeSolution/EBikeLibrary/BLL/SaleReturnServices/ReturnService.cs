using EBikeLibrary.DAL;
using EBikeLibrary.Entities;
using EBikeLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBikeLibrary.BLL.SaleReturnServices
{
    public class ReturnService
    {
        #region fields
        private readonly eBike_DMIT2018Context _eBikeContext;
        #endregion

        internal ReturnService(eBike_DMIT2018Context eBikeContext)
        {
            _eBikeContext = eBikeContext;
        }

        #region Lookups
        public SaleRefundView GetSaleRefund(int saleid)
        {
            if (saleid == 0)
            {
                throw new ArgumentNullException("Please provide a valid saleid");
            }
            SaleRefundView sales =
                _eBikeContext.Sales
                .Where(x => x.SaleId == saleid)
                .Select(x => new SaleRefundView
                {
                    SaleID = x.SaleId,
                    EmployeeID = x.EmployeeId,
                    TaxAmount = x.TaxAmount,
                    SubTotal = x.SubTotal,
                    DiscountPercent = x.Coupon.CouponDiscount

                }).FirstOrDefault();
            return sales;
        }
        public List<SaleRefundDetailView> GetSaleDetailsRefund(int saleid)
        {
            if (saleid == 0)
            {
                throw new ArgumentNullException("Please provide a valid saleid");
            }
            return _eBikeContext.SaleRefundDetails
                .Where(x => x.SaleRefund.SaleId == saleid)
                .Select(x =>
                new SaleRefundDetailView
                {
                    PartID = x.PartId,
                    Description = x.Part.Description,
                    OriginalQuantity = x.Quantity,
                    SellingPrice = x.SellingPrice,
                    Refundable = new(),
                    Quantity = new(),
                    Reason = x.Reason
                }).ToList();
        }
        #endregion
        #region command methods 
        public SaleRefund Refund(SaleRefundView saleRefund, List<SaleRefundDetailView> saleRefundDetails)
        {
            List<Exception> errorList = new List<Exception>();
            foreach (var saleRefundDetail in saleRefundDetails)
            {
                if (saleRefundDetail.ReturnQuantity < 0)
                {
                    errorList.Add(new Exception($"Quantity for item {saleRefundDetail.Description} is less than zero(0)"));
                }
                //quantity is greater than zero and no reason given
                //	errorList.Add(new Exception($"There is no reason given for item ()"));
                if (saleRefundDetail.Quantity > 0 && string.IsNullOrEmpty(saleRefundDetail.Reason))
                {
                    errorList.Add(new Exception($"There is no reason given for item {saleRefundDetail.Description}"));
                }
                //Quantity return is larger than(original quantity - return quantity)
                //	errorList.Add(new Exception($"Quantity for item is greater than the original quantity");
                if (saleRefundDetail.OriginalQuantity - saleRefundDetail.ReturnQuantity < 0)
                {
                    errorList.Add(new Exception($"Quantity for {saleRefundDetail.Description} is greater than the original quantity"));
                }
                //validate that item can be refundable (Parts.Refundable)
                //	no
                //		errorList.Add(new Exception($"item is not refundable"));
                if (!(Convert.ToString(_eBikeContext.Parts.Where(p => p.PartId == saleRefundDetail.PartID).Select(p => p.Refundable)) == "Y"))
                {
                    errorList.Add(new Exception($"{saleRefundDetail.Description} is not refundable"));
                }
            }
            if (errorList.Count > 0)
            {
                throw new AggregateException(errorList);
            }
            else
            {
                SaleRefund refund = new SaleRefund();
                refund.SaleRefundDate = DateTime.Now;
                //			SaleRefundDetails
                refund.SaleId = saleRefund.SaleID;
                refund.EmployeeId = saleRefund.EmployeeID;
                refund.TaxAmount = saleRefund.TaxAmount - saleRefund.DiscountPercent / 100 * saleRefund.TaxAmount;
                refund.SubTotal = saleRefund.SubTotal - saleRefund.DiscountPercent / 100 * saleRefund.SubTotal;
                _eBikeContext.SaleRefunds.Update(refund);
                foreach (var saleRefundDetail in saleRefundDetails)
                {
                    SaleRefundDetail returnDetail = new SaleRefundDetail();
                    returnDetail.PartId = saleRefundDetail.PartID;
                    returnDetail.Quantity = saleRefundDetail.ReturnQuantity;
                    returnDetail.SellingPrice = saleRefundDetail.SellingPrice;
                    returnDetail.Reason = saleRefundDetail.Reason;
                    _eBikeContext.SaleRefundDetails.Update(returnDetail);
                    //	Parts
                    //	Increase QunatityOnHand == QuantityOnHand + SaleRefundDetail.Quantity
                    Part part = _eBikeContext.Parts.Where(p => p.PartId == saleRefundDetail.PartID).Select(p => p).FirstOrDefault();
                    part.QuantityOnHand = part.QuantityOnHand + saleRefundDetail.Quantity;
                    _eBikeContext.Parts.Update(part);
                }
                return refund;
            }
        }
        #endregion

    }
}
