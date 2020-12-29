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
    public class HomeController :BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        HouseManageEntities hm = new HouseManageEntities();
        public string GetHouse()
        {
            int userId = ViewBag.userId;
            var room = hm.Room.Where(w => w.IsDelete != 0).ToList();
            var house = hm.House.Where(w => w.IsDelete != 0&&w.UserId==userId).OrderBy(o => o.Community).ThenBy(t=>t.Building);
            List<HousePage> model = new List<HousePage>();
            List<string> arr = new List<string>();
            foreach (var i in house)
            {
                HousePage hp = new HousePage();
                hp.house = i;
                hp.rooms = room.Where(s => s.HouseId == hp.house.Id).ToList();
                model.Add(hp);
                if (arr.IndexOf(i.FirstLetter) < 0)
                {
                    arr.Add(i.FirstLetter);
                }
            }
            if (model.Count > 0)
            {
                model[0].firstLetter = arr;
            }
            return JsonConvert.SerializeObject(model);
        }
        public string AddHouse(House house, List<Room> rooms)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            using (var transaction = hm.Database.BeginTransaction())
            {
                try
                {
                    if (house.Id == 0)
                    {
                        house.InsertDate = DateTime.Now;
                        house.FirstLetter = PingYinHelper.GetFirstLetter(house.Community);
                        house.Deadline = house.TakeDate.Value.AddMonths((int)house.Expiration).AddDays(-1);
                        //house.PayRentDate = house.PayRentDate;
                        //house.PayRentType = house.PayRentType;
                        //house.Deposit = house.Deposit;
                        //house.Remarks = house.Remarks;
                        house.UserId = userId;
                        var p = hm.Point.FirstOrDefault(f => f.Name == house.Community);
                        if (p != null)
                        {
                            house.Point = p.Point1;
                        }
                        hm.House.Add(house);
                        hm.SaveChanges();
                        if (house.PayMoney != null)
                        {
                            var expend = new Expend();
                            expend.InsertDate = DateTime.Now;
                            expend.HouseId = house.Id;
                            expend.UserId = userId;
                            expend.Money = house.PayMoney;
                            hm.Expend.Add(expend);
                        }                        
                        foreach (var r in rooms)
                        {
                            r.HouseId = house.Id;
                            r.Status = "0";
                        }
                        hm.Room.AddRange(rooms);
                    }
                    else
                    {
                        var ho = hm.House.FirstOrDefault(w => w.Id == house.Id);
                        //操作日志
                        Log log = new Log();
                        log.InsertDate = DateTime.Now;
                        log.UserId = userId;
                        log.Type = "房屋修改";
                        log.Old = "oldHouse:" + JsonConvert.SerializeObject(ho);
                        
                        ho.Community = house.Community;
                        ho.Building = house.Building;
                        ho.CostPrice = house.CostPrice;
                        ho.TakeDate = house.TakeDate;
                        ho.Expiration = house.Expiration;
                        ho.Deadline = ho.TakeDate.Value.AddMonths((int)ho.Expiration).AddDays(-1);
                        ho.Owner = house.Owner;
                        ho.PayRentDate = house.PayRentDate;
                        ho.Contact = house.Contact;
                        ho.PayRentType = house.PayRentType;
                        ho.Deposit = house.Deposit;
                        ho.Remarks = house.Remarks;
                        var roomlist = hm.Room.Where(w => w.HouseId == house.Id && w.IsDelete != 0);
                        //日志
                        log.New = JsonConvert.SerializeObject(roomlist);
                        hm.Log.Add(log);
                        foreach (var r in roomlist)
                        {
                            r.IsDelete = 0;
                        }
                        hm.SaveChanges();
                        foreach (var r in rooms)
                        {
                            if (r.Id == 0)
                            {
                                r.HouseId = house.Id;
                                hm.Room.Add(r);
                                r.Status = "0";
                            }
                            else
                            {
                                var ro = hm.Room.FirstOrDefault(f => f.Id == r.Id);
                                ro.Name = r.Name;
                                ro.Rent = r.Rent;
                                ro.Url = r.Url;
                                ro.IsDelete = null;
                            }

                        }
                    }
                    hm.SaveChanges();
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
        public string DelHouse(int id)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            using (var transaction = hm.Database.BeginTransaction())
            {
                try
                {
                    var house=hm.House.FirstOrDefault(f => f.Id == id);
                    house.IsDelete = 0;
                    hm.SaveChanges();
                    var rooms = hm.Room.Where(w => w.HouseId == id);
                    var roomIds = "";
                    foreach (var r in rooms)
                    {
                        r.IsDelete = 0;
                        roomIds += r.Id + ",";
                    }
                    //操作日志
                    Log log = new Log();
                    log.InsertDate = DateTime.Now;
                    log.UserId = userId;
                    log.Type = "房屋删除";
                    log.New = "houseId:" + house.Id + ";roomId:[" + roomIds+"]";
                    hm.Log.Add(log);
                    hm.SaveChanges();
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    res.Succ = false;
                    res.msg = ex.Message;
                }
            }
            return JsonConvert.SerializeObject(res);
        }
        public string EditRoom(Room room)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            try
            {
                var r = hm.Room.FirstOrDefault(f => f.Id == room.Id);
                //操作日志
                Log log = new Log();
                log.InsertDate = DateTime.Now;
                log.UserId = userId;
                log.Type = "room修改";
                log.Old = JsonConvert.SerializeObject(r);
                r.ContractDate = room.ContractDate;
                r.Deadline = room.Deadline;
                r.Rent = room.Rent;
                r.PayRentType = room.PayRentType;
                r.Deposit = room.Deposit;
                r.PayRentDate = room.PayRentDate;
                r.Remarks = room.Remarks;
                r.Tenants = room.Tenants;
                r.Contact = room.Contact;
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

        public string CreatTenant(int id,int type)
        {
            int userId = ViewBag.userId;
            Result res = new Result();
            res.Succ = true;
            using (var transaction = hm.Database.BeginTransaction())
            {
                try
                {
                    var room = hm.Room.FirstOrDefault(f => f.Id == id);
                    var ten = hm.Tenant.FirstOrDefault(f => f.Phone == room.Contact);
                    if (type == 0)
                    {
                        if (ten != null)
                        {
                            throw new Exception("该手机号已被使用！");
                        }
                        ten = new Tenant();
                        ten.Phone = room.Contact;
                        ten.PassWord = new GetMD5().MD5Encrypt64("123456");
                        ten.Name = room.Contact;
                        hm.Tenant.Add(ten);
                        hm.SaveChanges();
                    }
                    else
                    {
                        if (ten == null)
                        {
                            throw new Exception("用户不存在！");
                        }
                    }
                    room.TenantId = ten.Id;
                    hm.SaveChanges();
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
    }
}