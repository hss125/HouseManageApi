using HouseManage.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseManage.Models
{
    public class MyModel
    {
        public decimal? rev { get; set; }
        public int revCount { get; set; }
        public decimal? exp { get; set; }
        public Account acc { get; set; }
    }
}