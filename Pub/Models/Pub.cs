using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pub.Models
{
    public class Pub_JSON
    {
        public string WebPid1 { get; set; }
        public string program_no { get; set; }
    }

    public class Check_JSON
    {
        public bool Check { get; set; }
        public string Msg { get; set; }
    }

    public class Range_JSON
    {
        public string user_id { get; set; }
        public string program_no { get; set; }
        public string data { get; set; }
        public string tblname { get; set; }
        public string clnname { get; set; }
    }

    public class Select_JSON
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Stu_JSON
    {
        public string DeptId { get; set; }
        public int Secno { get; set; }
        public int Grade { get; set; }
        public int Clacod { get; set; }
        public int syearEnd { get; set; }
        public int semEnd { get; set; }
    }
}