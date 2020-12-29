using HouseManage.Models;
using HouseManage.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace HouseManage.Controllers
{
    public class MyController : BaseController
    {
        // GET: My
        HouseManageEntities ef = new HouseManageEntities();
        public string GetMy()
        {
            int userId = ViewBag.userId;
            var model = new MyModel();
            DateTime dtStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime dtEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, 1);
            var rev = ef.Revenue.Where(w => w.UserId == userId && w.InsertDate > dtStart && w.InsertDate < dtEnd);
            model.rev = rev.Sum(s => s.Money);
            model.revCount = rev.Count();
            model.exp = ef.Expend.Where(w => w.UserId == userId && w.InsertDate > dtStart && w.InsertDate < dtEnd).Sum(s => s.Money);
            model.acc = ef.Account.FirstOrDefault(f=>f.Id==userId);
            return JsonConvert.SerializeObject(model);
        }
        public string GetLog()
        {
            int userId = ViewBag.userId;
            var model = ef.Log.Where(w => w.UserId == userId).OrderByDescending(o=>o.Id).Take(100).ToList();
            foreach (var item in model)
            {
                if (item.Type == "收租")
                {
                    item.Old = item.Old.Replace("PayRentDate", "收租前应交日期") + item.New.Replace("PayRentDate", "收租后应交日期");
                }
                else if (item.Type == "出租")
                {
                    item.Old = item.New;
                }
                else if (item.Type == "退房")
                {
                    item.Old = item.New;
                }
                else if (item.Type == "交租")
                {
                    item.Old = item.Old.Replace("PayRentDate", "交租前应交日期") + item.New.Replace("PayRentDate", "交租后应交日期");
                }
            }
            return JsonConvert.SerializeObject(model);
        }
        public string BindOpenId(string code, int id)
        {
            Result res = new Result();
            res.Succ = true;
            var hr = new HttpResquet();
            string data = "?appid=wx4ce973a2bee533d8&secret=00e36484ccb55a9ac858df55f0ca7328&grant_type=authorization_code&js_code=" + code;
            try
            {
                var openInfo = hr.HttpGet("https://api.weixin.qq.com/sns/jscode2session" + data);
                var acc = ef.Account.FirstOrDefault(f => f.Id == id);
                var info = JsonConvert.DeserializeAnonymousType(openInfo, new { errcode = 0, openid = "", errmsg = "" });

                if (acc != null && info.errcode == 0)
                {
                    acc.OpenId = info.openid;
                    ef.SaveChanges();
                }
                else
                {
                    res.Succ = false;
                    res.msg = info.errmsg;
                }
            }
            catch (Exception ex)
            {
                res.Succ = false;
                res.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(res);
        }
    }
}