using HouseManage.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HouseManage.Controllers
{
    public class FileController : Controller
    {
        // GET: File
        public string ImgUpload()
        {
            Result res = new Result();
            res.Succ = true;
            try
            {
                HttpPostedFileBase file = Request.Files[0];
                var filename = DateTime.Now.ToString("yyyyMMddHHmmss") + file.FileName.Substring(file.FileName.LastIndexOf("."));
                string savePath = AppDomain.CurrentDomain.BaseDirectory + "Upload/Room/";
                if (Directory.Exists(savePath) == false)
                {
                    Directory.CreateDirectory(savePath);
                }
                file.SaveAs(savePath + filename);
                res.msg = filename;
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