using Models.DAO;
using Models.EF;
using ShopDienThoaiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ShopDienThoaiAPI.Controllers.Api
{
    //[Authorize]
    public class OrderController : ApiController
    {
        //[Authorize]
        [Route("api/order/getorderstatus")]
        public async Task<IEnumerable<ORDERSTATU>> GetOrderStatus()
        {
            return await new OrderStatusDAO().LoadStatus();
        }
        //[Authorize]
        [Route("api/order/getall")]
        public async Task<IEnumerable<ORDER>> GetAllOrder()
        {
            return await new OrderDAO().LoadOrder();
        }

        //[Authorize]
        [Route("api/order/getorder")]
        public async Task<IEnumerable<ORDER>> GetOrder(int customerid)
        {
            var item = await new OrderDAO().LoadOrder(customerid);
            return item;
        }

        //[Authorize]
        [Route("api/order/getorderlist")]
        public async Task<IEnumerable<ORDER>> GetOrderList(int customerid)
        {
            var list = await new OrderDAO().LoadOrder(customerid);
            return list;
        }

        //[Authorize]
        [Route("api/order/getproductlist")]
        public async Task<IQueryable<Object>> GetProductList(int orderid)
        {
            return await new OrderDAO().LoadProductOrder(orderid);
        }

        //[Authorize]
        [Route("api/order/getorderstatus")]
        public async Task<ORDERSTATU> GetOrderName(int orderid)
        {
            var item = await new OrderDAO().LoadByID(orderid);
            var statusname = await new OrderStatusDAO().LoadByID(item.OrderStatusID.Value);
            return statusname;
        }

        //[Authorize]
        [Route("api/order/updatestatus")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateOrderStatus(int orderid, ORDERSTATU status)
        {
            try
            {
                var result = await new OrderDAO().ChangeOrder(orderid, status.StatusID);
                if (result == 0)
                    return Content(HttpStatusCode.BadRequest, "Fail");
                else if (result == -1)
                    return Content(HttpStatusCode.BadRequest, "Error orcured");
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("api/order/create")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOrder(ORDER order)
        {
            try
            {
                var result = await new OrderDAO().CreateOrder(order);
                if (result > 0)
                    return Ok(result);
                else
                    return Content(HttpStatusCode.BadRequest, "Fail");

            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [Route("api/order/delete")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteOrder(int orderid)
        {
            try
            {
                var result = await new OrderDAO().DeleteOrder(orderid);
                if (result > 0)
                {
                    return Ok();
                }
                return Content(HttpStatusCode.Conflict, "Not deleted");
            }
            catch
            {
                return Content(HttpStatusCode.BadRequest, "Delete Fail");
            }
        }

        [Route("api/orderdetail/create")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOrderDetail(int orderid, List<CartItemModel> list)
        {
            try
            {
                foreach (var item in list)
                {
                    var result = await new OrderDetailDAO().AddOrderDetail(orderid, item.product.ProductID, item.quantity);
                    if (result == 0)
                    {
                        return Content(HttpStatusCode.BadRequest, "Fail");
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}
