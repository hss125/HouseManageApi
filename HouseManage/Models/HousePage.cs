using HouseManage.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseManage.Models
{
    public class HousePage
    {
        public  House house{ get; set; }
        public List<Room> rooms { get; set; }
        public List<string> firstLetter { get; set; }
    }
}