using HouseManage.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseManage.Models
{
    public class TenantCenter
    {
        public Tenant tenant { get; set; }
        public IQueryable<object> roomList { get; set; }
    }
}