using HouseManage.Models;
using HouseManage.Models.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HouseManage.Controllers
{
    public class OwnerController :BaseController
    {
        HouseManageEntities ef = new HouseManageEntities();
        public string GetTenant()
        {
            int userId = ViewBag.userId;
            int PayRentDays = Convert.ToInt32(ConfigurationManager.AppSettings["OwnerPayRentDays"]);
            int RenewalDays = Convert.ToInt32(ConfigurationManager.AppSettings["OwnerRenewalDays"]);
            OwnerModel model = new OwnerModel();
            model.payRent=ef.House.Where(w=>w.IsDelete!=0&&w.UserId==userId && SqlFunctions.DateDiff("DAY",DateTime.Now,w.PayRentDate)< PayRentDays);
            model.contractExpires = ef.House.Where(w => w.IsDelete != 0 && w.UserId == userId && SqlFunctions.DateDiff("DAY", DateTime.Now, w.Deadline) < RenewalDays);
            return JsonConvert.SerializeObject(model);
        }
        public string PayRent(House house)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            try
            {
                var r = ef.House.FirstOrDefault(f => f.Id == house.Id);
                Log log = new Log();
                log.InsertDate = DateTime.Now;
                log.UserId = userId;
                log.Type = "交租";
                log.Old = r.Community+r.Building+":houseId" + r.Id + ";PayRentDate:" + r.PayRentDate;
                r.PayRentDate = r.PayRentDate.Value.AddMonths((int)house.PayRentMonths);
                log.New = "收租月数:" + house.PayRentMonths + ";PayRentDate:" + r.PayRentDate;
                //记录交租
                var expend = new Expend();
                expend.InsertDate = DateTime.Now;
                expend.HouseId = house.Id;
                expend.UserId = userId;
                expend.Money = r.CostPrice * house.PayRentMonths;
                ef.Expend.Add(expend);
                ef.Log.Add(log);
                ef.SaveChanges();
            }
            catch (Exception ex)
            {
                res.Succ = false;
                res.msg = ex.Message;
            }
            return JsonConvert.SerializeObject(res);
        }
        public string Renewal(House house)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            try
            {
                var r = ef.House.FirstOrDefault(f => f.Id == house.Id);
                Log log = new Log();
                log.InsertDate = DateTime.Now;
                log.UserId = userId;
                log.Type = "房主续租";
                log.Old = "houseId:" + r.Id + ";Deadline:" + r.Deadline;
                ef.Log.Add(log);
                r.Deadline = r.Deadline.Value.AddMonths((int)house.Expiration);
                ef.SaveChanges();
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