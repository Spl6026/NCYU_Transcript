using FastReport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Student
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
    public class Courses
    {
        public string CourseId { get; set; }
        public string Course { get; set; }
        public int Credit { get; set; }
        public int Grade { get; set; }
        public int DataYear { get; set; }
        public int DataSemester { get; set; }
    }
}