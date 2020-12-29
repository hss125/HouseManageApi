using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseManage.Models
{
    public class RepairPrompting
    {
        public Senddata data { get; set; }
        public string page { get; set; }
        public string template_id { get; set; }
        public string touser { get; set; }
    }
    public class Senddata
    {
        public dataItem date1 { get; set; }
        public dataItem name2 { get; set; }
        public dataItem thing4 { get; set; }
        public dataItem thing5 { get; set; }
        public dataItem phone_number11 { get; set; }
    }
    public class dataItem
    {
        public string value { get; set; }
    }
}