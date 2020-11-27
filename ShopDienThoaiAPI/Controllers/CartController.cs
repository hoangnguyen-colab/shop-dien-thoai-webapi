using Models.DAO;
using Models.EF;
using Newtonsoft.Json;
using ShopDienThoaiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDienThoaiAPI.Controllers
{
    public class CartController : Controller
    {
        [Route("cart")]
        public ActionResult Cart()
        {
            return View((List<CartItemModel>)Session["cart"]);
        }

        [Route("checkout")]
        public async Task<ActionResult> Checkout()
        {
            if (Session["cart"] == null)
            {
                return RedirectToAction("Cart", "Cart");
            }
            var customer = HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(customer))
            {
                return View("Checkout", (List<CartItemModel>)Session["cart"]);
            }
            var item = await new CustomerDAO().LoadByUsername(customer);
            ViewData["Customer"] = item;
            return View("CheckoutCustomer", (List<CartItemModel>)Session["cart"]);
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> SubmitCheckoutCustomer(CheckoutModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = await GlobalVariable.GetCustomer(HttpContext.User.Identity.Name);
                if (customer == null)
                {
                    return Json(new JsonStatus()
                    {
                        Status = false,
                        Message = "Can not get your info, try to log in again",
                        StatusCode = -1
                    }, JsonRequestBehavior.AllowGet);

                }
                var order = new ORDER
                {
                    CustomerID = customer.CustomerID,
                    CustomerName = customer.CustomerName,
                    CustomerAddress = model.CustomerAddress,
                    CustomerPhone = model.CustomerPhone,
                    OrderStatusID = 1,
                    Total = await GetTotal(),
                    OrderDate = DateTime.Now
                };
                var json = JsonConvert.SerializeObject(order);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                string url = GlobalVariable.url + "api/order/create";

                try
                {
                    var client = new HttpClient();
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminController.AdminToken);
                    client.BaseAddress = new Uri(url);

                    var response = await client.PostAsync(url, data);
                    if (response.IsSuccessStatusCode)
                    {
                        var orderid = await response.Content.ReadAsStringAsync();
                        var result = AddOrderDetail(Convert.ToInt32(orderid));
                        return Json(new JsonStatus()
                        {
                            Status = true,
                            Message = "Create Success",
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
                        Message = "Error while creating your order",
                        StatusCode = 0
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new JsonStatus()
                {
                    Status = false,
                    Message = "Invalid input"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> SubmitCheckout(CheckoutModel model)
        {
            if (ModelState.IsValid)
            {
                var order = new ORDER
                {
                    CustomerName = model.CustomerName,
                    CustomerAddress = model.CustomerAddress,
                    CustomerPhone = model.CustomerPhone,
                    OrderStatusID = 1,
                    Total = await GetTotal(),
                    OrderDate = DateTime.Now
                };
                var json = JsonConvert.SerializeObject(order);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                string url = GlobalVariable.url + "api/order/creat";

                try
                {
                    var client = new HttpClient();
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminController.AdminToken);
                    client.BaseAddress = new Uri(url);

                    var response = await client.PostAsync(url, data);
                    if (response.IsSuccessStatusCode)
                    {
                        //return Json(new JsonStatus()
                        //{
                        //    Status = true,
                        //    Message = "Create Success",
                        //    StatusCode = (int)response.StatusCode
                        //}, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new JsonStatus()
                        {
                            Status = false,
                            Message = response.ReasonPhrase,
                            StatusCode = (int)response.StatusCode
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new JsonStatus()
                    {
                        Status = false,
                        Message = "Error while creating your order",
                        StatusCode = 0
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new JsonStatus()
            {
                Status = false,
                Message = "Invalid input"
            }, JsonRequestBehavior.AllowGet);
        }

        private async Task<bool> AddOrderDetail(int orderid)
        {
            try
            {
                var orderdetail = new OrderDetailDAO();
                foreach (var item in (List<CartItemModel>)Session["cart"])
                {
                    await orderdetail.AddOrderDetail(orderid, item.product.ProductID, item.quantity);
                }
                Session["cart"] = null;
                return true;
            }
            catch
            {
                var result = await new OrderDAO().DeleteOrder(orderid);
                return false;
            }
        }

        [HttpPost]
        [Route("addtocart")]
        public async Task<JsonResult> OrderNow(int prodId, int quantity)
        {
            //get product by id
            var prod = await GetData(prodId);
            //check null
            if (prod == null)
            {
                return Json(new JsonStatus()
                {
                    Status = false,
                    Message = "Not available"
                }, JsonRequestBehavior.AllowGet);
            }
            //check quantity 
            if (quantity <= 0)
            {
                return Json(new JsonStatus()
                {
                    Status = false,
                    Message = "So luong khong the nho hon 0"
                }, JsonRequestBehavior.AllowGet);
            }
            //check session exist
            if (Session["cart"] == null)
            {
                //add new if null
                var cart = new List<CartItemModel>();
                cart.Add(new CartItemModel(prod, quantity));
                Session["cart"] = cart;

                return Json(new JsonStatus()
                {
                    Status = true,
                    Message = "Add moi"
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var cart = (List<CartItemModel>)Session["cart"];
                //check product exist in session
                int index = IsExist(prodId);
                if (index == -1)
                {
                    //add new if product not exist
                    cart.Add(new CartItemModel(prod, quantity));
                    return Json(new JsonStatus()
                    {
                        Status = true,
                        Message = "Add moi"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //append quantity if product exist
                    cart[index].quantity += quantity;
                    return Json(new JsonStatus()
                    {
                        Status = true,
                        Message = "Add so luong"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public async Task<PRODUCT> GetData(int id)
        {
            try
            {
                string apiurl = GlobalVariable.url + "api/product/detail?id={0}";
                apiurl = string.Format(apiurl, id);

                string json = await new GlobalVariable().GetApiAsync(apiurl);
                var prod = JsonConvert.DeserializeObject<PRODUCT>(json);

                return prod;
            }
            catch
            {
                return null;
            }
        }

        public ActionResult Delete(int id)
        {
            int index = IsExist(id);
            List<CartItemModel> cart = (List<CartItemModel>)Session["cart"];
            cart.RemoveAt(index);
            if (cart.Count == 0)
            {
                Session["cart"] = null;
            }
            return RedirectToAction("Cart", "Cart");
        }

        private int IsExist(int id)
        {
            var cart = (List<CartItemModel>)Session["cart"];
            for (int i = 0; i < cart.Count; i++)
                if (cart[i].product.ProductID == id)
                    return i;
            return -1;
        }

        public ActionResult CartPartial()
        {
            return PartialView("_Cart", (List<CartItemModel>)Session["cart"]);
        }

        public async Task<decimal> GetTotal()
        {
            decimal total = 0;

            var cart = (List<CartItemModel>)Session["cart"];
            if (cart != null)
            {
                var dao = new ProductDAO();
                foreach (var item in cart)
                {
                    var product = await dao.LoadByID(item.product.ProductID);
                    if (product.PromotionPrice.HasValue)
                    {
                        total += product.PromotionPrice.Value * item.quantity;
                    }
                    else
                    {
                        total += product.ProductPrice * item.quantity;
                    }
                }
            }
            return total;
        }
    }
}