using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HouseManage.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.userId = -1;
            var tick = Request.QueryString["ticket"];
            if (!string.IsNullOrEmpty(tick))
            {
                FormsAuthenticationTicket authTicket = null;
                authTicket = FormsAuthentication.Decrypt(tick);
                if (!authTicket.Expired)
                {
                    var user = new Models.EF.HouseManageEntities().Account.FirstOrDefault(f=>f.Name==authTicket.Name&&f.IsDelete!=0);
                    if (user != null)
                    {
                        ViewBag.userId = user.Id;
                    }                    
                }
            }
        }
    }
}