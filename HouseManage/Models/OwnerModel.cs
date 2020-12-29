using HouseManage.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseManage.Models
{
    public class OwnerModel
    {
        public IQueryable<House> payRent{ get; set; }
        public IQueryable<House> contractExpires { get; set; }
    }
}