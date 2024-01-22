using System.Diagnostics;
using System.Globalization;
using System;
using System.Data.SqlClient;

namespace Calculate.Models
{
    public class JSON
    {
        public string user_id { get; set; }
        public string DeptId { get; set; }
        public string StudentId { get; set; }
        public int Secno { get; set; }
        public int Grade { get; set; }
        public int Clacod { get; set; }
        public int syearEnd { get; set; }
        public int semEnd { get; set; }
        public bool Isrank { get; set; }
    }

    public class Student
    {
        public string stuno { get; set; }
        public decimal scoavg { get; set; }
        public int rank { get; set; }
    }

    public class Courses
    {
        public bool Pass {  get; set; }
        public decimal Credit { get; set; }
        public decimal Grade { get; set; }
        public int dropcd { get; set; }
    }

    public class RegSem
    {
        public int syear { get; set; }
        public int sem { get; set; }
        public string deptno { get; set; }
        public int secno { get; set; }
        public int grade { get; set; }
        public int clacod { get; set; }
    }
}