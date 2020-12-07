using Models.DAO;
using Models.EF;
using Newtonsoft.Json;
using ShopDienThoaiAPI.Common;
using ShopDienThoaiAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDienThoaiAPI.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    public class OrderedController : BaseController
    {
        public ActionResult Order()
        {
            return View();
        }

        public async Task<ActionResult> OrderList()
        {
            //var list = new List<ORDERSTATU>();
            //foreach (var item in await new OrderStatusDAO().LoadStatus())
            //{
            //    if (item.StatusID != 5)
            //    {
            //        list.Add(item);
            //    }
            //}
            //ViewBag.Status = list;
            string url = GlobalVariable.url + "api/order/getall";

            string json = await new GlobalVariable().GetApiAsync(url);
            var list = JsonConvert.DeserializeObject<List<ORDER>>(json);

            return PartialView("OrderList", list);
        }

        public async Task<ActionResult> OrderDetail(int orderid)
        {
            var item = await new OrderDAO().LoadByID(orderid);
            return View(item);
        }

        public async Task<JsonResult> OrderProductDetail(int orderid)
        {
            var item = await new OrderDAO().LoadProductOrder(orderid);
            return Json(item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> EditOrder(int Orderid, int StatusID)
        {
            var json = JsonConvert.SerializeObject(new ORDERSTATU()
            {
                StatusID = StatusID
            });
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            string url = GlobalVariable.url + "api/order/updatestatus?orderid=" + Orderid;

            try
            {
                var client = new HttpClient();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminController.AdminToken);
                client.BaseAddress = new Uri(url);

                var response = await client.PutAsync(url, data);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new JsonStatus()
                    {
                        Status = true,
                        Message = "Update Success",
                        StatusCode = (int)response.StatusCode
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new JsonStatus()
                    {
                        Status = false,
                        Message = await response.Content.ReadAsStringAsync(),
                        StatusCode = (int)response.StatusCode
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new JsonStatus()
                {
                    Status = false,
                    Message = "Error while updating brand",
                    StatusCode = 0
                }, JsonRequestBehavior.AllowGet);
            }
            //try
            //{
            //    api / order / updatestatus
            //    int result = await new OrderDAO().ChangeOrder(Orderid, StatusID);
            //    if (result == 0)
            //    {
            //        return Json(new { Success = false, Orderid }, JsonRequestBehavior.AllowGet);
            //    }
            //    return Json(new { Success = true, Orderid }, JsonRequestBehavior.AllowGet);
            //}
            //catch
            //{
            //    return Json(new { Success = false, Orderid }, JsonRequestBehavior.AllowGet);
            //}
        }

        public async Task<JsonResult> StatusList(int id)
        {
            string url = GlobalVariable.url + "api/order/getorderstatus";

            string json = await new GlobalVariable().GetApiAsync(url);
            var statuslist = JsonConvert.DeserializeObject<List<ORDERSTATU>>(json);
            if (statuslist != null)
            {
                if (id == 2)
                {
                    var list = new List<ORDERSTATU>();
                    list.Add(statuslist.Where(x => x.StatusID == 3).FirstOrDefault());
                    list.Add(statuslist.Where(x => x.StatusID == 4).FirstOrDefault());

                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else if (id == 1)
                {
                    var list = new List<ORDERSTATU>();
                    foreach (var item in statuslist)
                    {
                        if (item.StatusID != 1)
                        {
                            list.Add(item);
                        }
                    }
                    return Json(statuslist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { statuslist.Where(x => x.StatusID == id).FirstOrDefault().StatusName }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Status = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}