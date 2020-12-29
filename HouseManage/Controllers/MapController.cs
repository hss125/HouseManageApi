using HouseManage.Models;
using HouseManage.Models.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HouseManage.Controllers
{
    public class MapController : Controller
    {
        // GET: Map
        public ActionResult Index()
        {
            return View();
        }
        HouseManageEntities ef = new HouseManageEntities();
        public string GetRentRoom()
        {
            Result res = new Result();
            res.Succ = true;
            var select = from room in ef.Room
                         join house in ef.House on room.HouseId equals house.Id
                         join acc in ef.Account on house.UserId equals acc.Id
                         where room.Status != "1" &&room.IsDelete!=0 && house.Point!=null && acc.IsShow==1
                         select new
                         {
                             room,
                             acc.RealName,
                             acc.Name,
                             acc.Phone,
                             house.Community,
                             house.Building,
                             house.Point
                         };
            return JsonConvert.SerializeObject(select);
        }
    }
}