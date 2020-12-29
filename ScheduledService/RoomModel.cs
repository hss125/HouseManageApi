using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledService
{
    class RoomModel
    {
        public string Building { get; set; }
        public string Community { get; set; }
        public Room room { get; set; }
        public Tenant tenant { get; set; }

    }
}
