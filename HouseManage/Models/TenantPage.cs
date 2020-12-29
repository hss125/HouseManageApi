using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseManage.Models
{
    public class TenantPage
    {
        public IQueryable<object> forRent { get; set; }
        public IQueryable<object> payRent { get; set; }
        public IQueryable<object> contractExpires { get; set; }
        public IQueryable<object> repair { get; set; }
    }
}