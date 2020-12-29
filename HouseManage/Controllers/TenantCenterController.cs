using HouseManage.Models;
using HouseManage.Models.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HouseManage.Controllers
{
    public class TenantCenterController : Controller
    {
        HouseManageEntities ef = new HouseManageEntities();
        // GET: TenantCenter
        public string GetTenant(int id)
        {
            TenantCenter model = new TenantCenter();
            model.tenant = ef.Tenant.FirstOrDefault(f => f.Id == id);
            var select = from room in ef.Room
                         join house in ef.House on room.HouseId equals house.Id
                         join acc in ef.Account on house.UserId equals acc.Id
                         where room.TenantId==id
                         select new { acc,room.PayRentType,room.PayRentDate,room.Rent,room.Deposit,
                         room.Deadline,room.Name,room.Id,house.Community,house.Building};
            model.roomList = select;
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
                var ten = ef.Tenant.FirstOrDefault(f => f.Id == id);
                var info = JsonConvert.DeserializeAnonymousType(openInfo, new { errcode = 0, openid = "", errmsg = "" });

                if (ten != null && info.errcode == 0)
                {
                    ten.OpenId = info.openid;
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
        public string EditInfo(Tenant ten)
        {
            Result res = new Result();
            res.Succ = true;
            using (var transaction = ef.Database.BeginTransaction())
            {
                try
                {
                    var tenant = ef.Tenant.FirstOrDefault(f => f.Id == ten.Id);
                    if (tenant.Phone != ten.Phone)
                    {
                        var ten1 = ef.Tenant.FirstOrDefault(f=>f.Phone==ten.Phone);
                        if (ten1 != null)
                        {
                            throw new Exception("该手机号已被使用！");
                        }
                    }
                    tenant.Name = ten.Name;
                    tenant.Phone = ten.Phone;
                    if (!string.IsNullOrEmpty(ten.PassWord))
                    {
                        tenant.PassWord = new GetMD5().MD5Encrypt64(ten.PassWord);
                    }
                    ef.SaveChanges();
                    var rooms = ef.Room.Where(w=>w.TenantId==ten.Id);
                    if (rooms.Count() > 0)
                    {
                        foreach (var r in rooms)
                        {
                            r.Tenants = ten.Name;
                            r.Contact = ten.Phone;
                        }
                        ef.SaveChanges();
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
        public string Repair(Repair rep, int id)
        {
            Result res = new Result();
            res.Succ = true;
            var room = ef.Room.FirstOrDefault(f=>f.Id== id);
            var date1 = DateTime.Now.AddDays(-3);
            var reps = ef.Repair.Where(w => w.RoomId == room.Id && w.InsertDate > date1);
            if (reps.Count() < 3)
            {
                try
                {
                    var house = ef.House.FirstOrDefault(f => f.Id == room.HouseId);
                    rep.InsertDate = DateTime.Now;
                    rep.RoomId = room.Id;
                    rep.LandlordId = house.UserId;
                    ef.Repair.Add(rep);
                    ef.SaveChanges();
                    SendEmail(house, room, rep.Detail);
                }

                catch (Exception ex)
                {
                    res.Succ = false;
                    res.msg = ex.Message;
                }
                
            }
            else {
                res.Succ = false;
                res.msg = "一周内报修不能超过3次！";
            }
            
            return JsonConvert.SerializeObject(res);
        }
        public string SendMess(House house,Room room)
        {
            var ten = ef.Tenant.FirstOrDefault(f=>f.Id==room.TenantId);
            var acc= ef.Account.FirstOrDefault(f => f.Id == house.UserId);
            string msg = "";
            HttpResquet resquet = new HttpResquet();
            var type = JsonConvert.DeserializeAnonymousType(resquet.HttpGet("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wx4ce973a2bee533d8&secret=00e36484ccb55a9ac858df55f0ca7328"), new
            {
                errcode = 0,
                access_token = "",
                errmsg = ""
            });
            if (type.errcode == 0)
            {
                string postUrl = "https://api.weixin.qq.com/cgi-bin/message/subscribe/send?access_token=" + type.access_token;
                RepairPrompting sendmodel = new RepairPrompting();
                sendmodel.template_id = "2jXJDSxgGNoD4XC9xBifq_PRZlrsNYEmiYtyN8f0w7o";
                sendmodel.touser = acc.OpenId;
                sendmodel.page = "";
                Senddata senddata1 = new Senddata();
                dataItem item1 = new dataItem();
                item1.value = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                senddata1.date1 = item1;
                sendmodel.data = senddata1;
                dataItem item2 = new dataItem();
                item2.value = "租客";
                senddata1.name2 = item2;
                dataItem item3 = new dataItem();
                item3.value = house.Community+house.Building;
                senddata1.thing4 = item3;
                dataItem item4 = new dataItem();
                item4.value = "打开小程序查看报修详情请";
                senddata1.thing5 = item4;
                dataItem item5 = new dataItem();
                item5.value = ten.Phone;
                senddata1.phone_number11 = item5;
                sendmodel.data = senddata1;
                var type2 = JsonConvert.DeserializeAnonymousType(resquet.HttpPost(postUrl, JsonConvert.SerializeObject(sendmodel), Encoding.UTF8), new
                {
                    errcode = 0,
                    errmsg = ""
                });
                if (type2.errcode != 0)
                {
                    msg = "发送失败:"+type2.errmsg;
                }
            }
            else
            {
                msg = "获取token失败:" + type.errmsg;
            }
            return msg;
        }
        public void SendEmail(House house, Room room,string content)
        {
            try
            {
                //smtp.163.com
                //string senderServerIp = "123.125.50.133";
                //smtp.gmail.com
                //string senderServerIp = "74.125.127.109";
                //smtp.qq.com
                string senderServerIp = "smtp.qq.com";
                //string senderServerIp = "smtp.sina.com";
                string toMailAddress = "a15000544615@qq.com";
                string fromMailAddress = "274852493@qq.com";
                string subjectInfo = "报修提醒";
                string bodyInfo = $"<h2>{house.Community+house.Building+room.Name}:</h2><div>{content}</div>";
                string mailUsername = "274852493@qq.com";
                string mailPassword = "ugxypxsdsykmbgba"; //发送邮箱的密码
                string mailPort = "25";

                MyEmail email = new MyEmail(senderServerIp, toMailAddress, fromMailAddress, subjectInfo, bodyInfo, mailUsername, mailPassword, mailPort, false, false);
                email.Send();
            }
            catch (Exception ex)
            {

            }
        }
    }
}