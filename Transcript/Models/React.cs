using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transcript.Models
{
    public class Select_JSON
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Sec_JSON
    {
        public string Id { get; set; }
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

    public class React
    {
    }
}