using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledService
{
    class Sendmodel
    {
        public Senddata data { get; set; }
        public string page { get; set; }
        public string template_id { get; set; }
        public string touser { get; set; }
    }
    class Senddata
    {
        public dataItem amount2 { get; set; }
        public dataItem date3 { get; set; }
        public dataItem name4 { get; set; }
        public dataItem thing1 { get; set; }
    }
    class dataItem
    {
        public string value { get; set; }
    }
}
