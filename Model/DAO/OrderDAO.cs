using Model.EF.CustomModel;
using Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DAO
{
    public class OrderDAO
    {
        ShopDienThoaiAPI db = null;
        public OrderDAO()
        {
            db = new ShopDienThoaiAPI();
        }

        public async Task<int> CreateOrder(ORDER order)
        {
            try
            {
                db.ORDERs.Add(order);
                await db.SaveChangesAsync();
                return order.OrderID;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<ORDER> LoadByID(int OrderID)
        {
            return await db.ORDERs.AsNoTracking().Where(x => x.OrderID == OrderID).FirstOrDefaultAsync();
        }

        public async Task<List<ORDER>> LoadOrder()
        {
            return await db.ORDERs.AsNoTracking().ToListAsync();
        }

        public async Task<List<ORDER>> LoadOrder(int CustomerID)
        {
            return await db.ORDERs.AsNoTracking().Where(x => x.CustomerID == CustomerID).ToListAsync();
        }

        public async Task<IQueryable<Object>> LoadProductOrder(int OrderID)
        {
            var query = from detail in db.ORDERDETAILs
                        join prod in db.PRODUCTs on detail.ProductID equals prod.ProductID
                        where detail.OrderID == OrderID
                        select new
                        {
                            Detail = detail,
                            productid = prod.ProductID,
                            productname = prod.ProductName,
                            productprice = prod.ProductPrice,
                            discount = prod.PromotionPrice,
                            imageurl = prod.ProductImage
                        };

            return query;
        }

        public async Task<int> ChangeOrder(int OrderID, int StatusID)
        {
            try
            {
                var order = await db.ORDERs.FindAsync(OrderID);
                if (order == null)
                {
                    return 0;
                }
                var status = await db.ORDERSTATUS.FindAsync(StatusID);
                if (status == null)
                {
                    return 0;
                }
                if (order.OrderStatusID == StatusID)
                {
                    return 0;
                }
                order.OrderStatusID = StatusID;
                await db.SaveChangesAsync();
                return order.OrderID;
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteOrder(int OrderID)
        {
            try
            {
                var order = await db.ORDERs.FindAsync(OrderID);
                if (order == null)
                    return 0;

                db.ORDERs.Remove(order);
                await db.SaveChangesAsync();
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<Report>> LoadReport()
        {
            string sql = @"
                SELECT sum(ORDERDETAIL.Quantity * PRODUCT.ProductPrice) AS Total, CONVERT(VARCHAR(10), OrderDate, 111) AS OrderDate
                FROM [ORDER]
                JOIN  ORDERDETAIL ON [ORDER].OrderID = ORDERDETAIL.OrderID
                JOIN PRODUCT ON ORDERDETAIL.ProductID = PRODUCT.ProductID
                GROUP BY OrderDate";

            var query = await new ShopDienThoaiAPI().Database.SqlQuery<Report>(sql).ToListAsync();

            return query;
        }
    }
}
