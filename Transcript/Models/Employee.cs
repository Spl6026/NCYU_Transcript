using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Employee
    {
        public string Name { get; set; }
        public string Birth { get; set; }
        public string Enrolled { get; set; }
        public string DegreeConferred { get; set; }
        public string DateConferred { get; set; }
        public string Department { get; set; }
        public string College { get; set; }
        public string Issued { get; set; }

    }
    public class Employeesub
    {
        public string sub { get; set; }
        public string sco { get; set; }

    }
}