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
    public class TenantController :BaseController
    {
        // GET: Tenant
        public ActionResult Index()
        {
            return View();
        }
        HouseManageEntities hm = new HouseManageEntities();
        public string GetTenant()
        {
            int userId = ViewBag.userId;
            int PayRentDays = Convert.ToInt32(ConfigurationManager.AppSettings["PayRentDays"]);
            int RenewalDays = Convert.ToInt32(ConfigurationManager.AppSettings["RenewalDays"]);
            var forRent = from room in hm.Room
                          join house in hm.House
                          on room.HouseId equals house.Id
                          where room.Status == "0" && room.IsDelete != 0  &&house.UserId==userId
                          select new { room, house };
            var payRent = from room in hm.Room
                          join house in hm.House
                          on room.HouseId equals house.Id
                          where SqlFunctions.DateDiff("DAY",DateTime.Now, room.PayRentDate) < PayRentDays
                          && room.IsDelete != 0 &&room.Status=="1" && house.UserId == userId
                          select new { room, house };
            var contractExpires = from room in hm.Room
                          join house in hm.House
                          on room.HouseId equals house.Id
                          where SqlFunctions.DateDiff("DAY", DateTime.Now, room.Deadline) < RenewalDays
                          && room.IsDelete != 0 && room.Status == "1" && house.UserId == userId
                                  select new { room, house };
            var rep = from repa in hm.Repair
                      join room in hm.Room
                      on repa.RoomId equals room.Id
                      join house in hm.House
                      on room.HouseId equals house.Id
                      where repa.LandlordId == userId && repa.Status != 0
                      select new { repa,room.Name,room.Tenants,room.Contact,house.Community,house.Building};
            TenantPage model = new TenantPage();
            model.forRent = forRent;
            model.payRent = payRent;
            model.contractExpires = contractExpires;
            model.repair = rep;
            return JsonConvert.SerializeObject(model);
        }
        /// <summary>
        /// 出租
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public string RentOut(Room room)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            using (var transaction = hm.Database.BeginTransaction())
            {
                try
                {
                    var r = hm.Room.FirstOrDefault(f => f.Id == room.Id);
                    var h = hm.House.FirstOrDefault(f => f.Id == r.HouseId);
                    r.ContractDate = room.ContractDate;
                    r.Expiration = room.Expiration;
                    r.Deadline = room.ContractDate.Value.AddMonths((int)room.Expiration).AddDays(-1);
                    r.PayRentDate = room.PayRentDate;
                    r.PayRentType = room.PayRentType;
                    r.Rent = room.Rent;
                    r.Deposit = room.Deposit;
                    r.Tenants = room.Tenants;
                    r.Contact = room.Contact;
                    r.Status = "1";
                    hm.SaveChanges();
                    Log log = new Log();
                    log.InsertDate = DateTime.Now;
                    log.UserId = userId;
                    log.Type = "出租";
                    log.New = h.Community+h.Building+r.Name+":roomId"+r.Id;
                    hm.Log.Add(log);
                    hm.SaveChanges();
                    //记录出租
                    if (room.IsDelete != null)
                    {
                        var rev = new Revenue();
                        rev.InsertDate = DateTime.Now;
                        rev.UserId = userId;
                        rev.RoomId = r.Id;
                        rev.Money = r.IsDelete;
                        hm.Revenue.Add(rev);
                        hm.SaveChanges();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    res.Succ = false;
                    res.msg = ex.Message;
                }
            }
                
            
            return JsonConvert.SerializeObject(res);
        }
        public string PayRent(Room room)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            try
            {
                var r = hm.Room.FirstOrDefault(f => f.Id == room.Id);
                var h = hm.House.FirstOrDefault(f => f.Id == r.HouseId);
                Log log = new Log();
                log.InsertDate = DateTime.Now;
                log.UserId = userId;
                log.Type = "收租";
                log.Old = h.Community+h.Building+r.Name+":roomId" + r.Id + ";PayRentDate:" + r.PayRentDate;
                r.PayRentDate = r.PayRentDate.Value.AddMonths((int)room.PayRentMonths);
                log.New = "收租月数:"+ room.PayRentMonths + ";PayRentDate:" + r.PayRentDate;
                //记录收租
                var rev = new Revenue();
                rev.InsertDate = DateTime.Now;
                rev.UserId = userId;
                rev.RoomId = r.Id;
                rev.Money = r.Rent * room.PayRentMonths;
                hm.Revenue.Add(rev);
                hm.Log.Add(log);
                hm.SaveChanges();
            }
            catch (Exception ex)
            {
                res.Succ = false;
                res.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(res);
        }
        public string contractRenewal(Room room)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            try
            {
                var r=hm.Room.FirstOrDefault(f => f.Id == room.Id);
                //操作日志
                Log log = new Log();
                log.InsertDate = DateTime.Now;
                log.UserId = userId;
                log.Type = "租客续租";
                log.Old = "roomId:" + r.Id + ";Deadline:" + r.Deadline+ ";Rent:"+ r.Rent;
                hm.Log.Add(log);
                r.Deadline = r.Deadline.Value.AddMonths((int)room.Expiration);
                r.Rent = room.Rent;
                hm.SaveChanges();
            }
            catch (Exception ex)
            {
                res.Succ = false;
                res.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(res);
        }
        /// <summary>
        /// 退房
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string CheckOut(int id)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            try
            {
                var r = hm.Room.FirstOrDefault(f => f.Id == id);
                var h = hm.House.FirstOrDefault(f => f.Id == r.HouseId);
                r.Status = "0";
                r.TenantId = 0;
                var t=hm.Tenant.FirstOrDefault(f => f.Id == r.TenantId);
                Log log = new Log();
                log.InsertDate = DateTime.Now;
                log.UserId = userId;
                log.Type = "退房";
                log.New = h.Community+h.Building+r.Name+":roomId"+r.Id+";tenantId:"+ (t==null?0:t.Id);
                hm.Log.Add(log);
                hm.SaveChanges();
            }
            catch (Exception ex)
            {
                res.Succ = false;
                res.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(res);
        }
        public string Repaired(int id)
        {
            Result res = new Result();
            res.Succ = true;
            var rep = hm.Repair.FirstOrDefault(f=>f.Id==id);
            rep.Status = 0;
            rep.FinishDate = DateTime.Now;
            hm.SaveChanges();
            return JsonConvert.SerializeObject(res);
        }
    }
}