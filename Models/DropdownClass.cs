using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Login_Application.Models
{
    public class DropdownClass
    {
        Login_ApplicationEntities db = new Login_ApplicationEntities();
        public DropdownClass()
        {
            this.Devices = new List<Device>();
        }
        public List<Device> Devices { set; get; }

        public int Oid { get; set; }
        public int Lid { get; set; }
        public int Sid { get; set; }

        public int Did { get; set; }
    }
}