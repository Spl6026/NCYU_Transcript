﻿using FastReport.Data;
using FastReport.Format;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.IdentityModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Transcript.Models
{
    public class JSON
    {
        public string StudentId { get; set; }
        public int syearEnd { get; set; }
        public int semEnd { get; set; }
        public bool Isrank { get; set; }
    }

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
        public decimal Score { get; set; }
        public decimal Credits { get; set; }
        //public decimal Rank { get; set; }
        //public decimal Class { get; set; }
        public string Percent { get; set; }
        public decimal GPA { get; set; }
        public bool IsRank { get; set; }
    }

    public class Courses
    { 
        public string Course { get; set; }
        public decimal Credit { get; set; }
        public bool Pass { get; set; }
        public decimal Grade { get; set; }
        public int DataYear { get; set; }
        public int DataSemester { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Average { get; set; }
        public decimal ConductMark { get; set; }
    }

    public class Sign
    {
        public byte[] SignReg { get; set; }
        public byte[] SignDean { get; set; }
        public Sign(string path)
        {
            SignReg = File.ReadAllBytes(Path.Combine(path, "signreg.png"));
            SignDean = File.ReadAllBytes(Path.Combine(path, "signdean.png"));
        }
    }

    public class DataBase
    {
        public static string TW2AD(string tw)
        {
            string TwYear = tw.Substring(0, 3);
            string Date = tw.Substring(3);
            string ad = (int.Parse(TwYear) + 1911).ToString() + Date;
            return ad;
        }

        public Tuple<decimal, decimal, string, decimal> Grades(List<Models.Courses> course) 
        {
            decimal Scores = 0;
            decimal suCredits = 0;
            decimal rgCredits = 0;
            string Percent = "0/0 (0.00%)";
            decimal GPA = 0;
            
            //Rank + "/" + Class + "(" + (Rank / (Class == 0 ? 1 : Class)).ToString("0.00%") + ")";
            
            foreach (var item in course)
            {
                Scores += item.Grade * item.Credit;

                suCredits += item.Credit;

                if (item.Grade >= 60)
                    rgCredits += item.Credit;

                if(item.Grade >= 80) 
                    GPA += 4 * item.Credit;
                
                else if(item.Grade >= 70) 
                    GPA += 3 * item.Credit;
                
                else if(item.Grade >= 60)
                    GPA += 2 * item.Credit;
                
                else if(item.Grade >= 50)
                    GPA += 1 * item.Credit;
               
            }
            return Tuple.Create(Math.Round(Scores / suCredits, 2), rgCredits, Percent, Math.Round(GPA / suCredits, 2));
        }

        public Tuple<List<Models.Student>, List<Models.Courses>> SQLGet(string StudentId, int syearEnd, int semEnd, bool Isrank)
        {
            List<Models.Student> stu = new List<Models.Student>();
            List<Models.Courses> course = new List<Models.Courses>();
            string connectionString = "Data Source=SPL\\SQLEXPRESS;Initial Catalog=Test_ncyu_dev;Persist Security Info=True;User ID=ccadmsup;Password=ccap2dev98";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string Year = "'AND (([Test_ncyu_dev].[dbo].[selstch].[syear] < " + syearEnd + ") OR ([Test_ncyu_dev].[dbo].[selstch].[syear] = " + syearEnd + " AND [Test_ncyu_dev].[dbo].[selstch].[sem] <= " + semEnd + "))";
                string cmd = "SELECT [englishco], [credit], [pass], [totalscore], [Test_ncyu_dev].[dbo].[selstch].[syear], [Test_ncyu_dev].[dbo].[selstch].[sem], [rgcrd], [scoavg], [total_score] FROM [Test_ncyu_dev].[dbo].[selstch] JOIN [Test_ncyu_dev].[dbo].[crscourse] ON [Test_ncyu_dev].[dbo].[selstch].[cono] = [Test_ncyu_dev].[dbo].[crscourse].[cono] LEFT JOIN [Test_ncyu_dev].[dbo].[con_behavior] ON [Test_ncyu_dev].[dbo].[selstch].[syear] = [Test_ncyu_dev].[dbo].[con_behavior].[syear] AND [Test_ncyu_dev].[dbo].[selstch].[sem] = [Test_ncyu_dev].[dbo].[con_behavior].[sem] AND [Test_ncyu_dev].[dbo].[selstch].[stuno] = [Test_ncyu_dev].[dbo].[con_behavior].[stuno]  LEFT JOIN [Test_ncyu_dev].[dbo].[selstchf] ON [Test_ncyu_dev].[dbo].[selstch].[syear] = [Test_ncyu_dev].[dbo].[selstchf].[syear] AND [Test_ncyu_dev].[dbo].[selstch].[sem] = [Test_ncyu_dev].[dbo].[selstchf].[sem] AND [Test_ncyu_dev].[dbo].[selstch].[stuno] = [Test_ncyu_dev].[dbo].[selstchf].[stuno] WHERE [Test_ncyu_dev].[dbo].[selstch].[stuno] = '" + StudentId + Year + " ORDER BY [syear], [sem]";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(2))
                            {
                                course.Add(new Models.Courses()
                                {
                                    Course = reader.GetString(0),
                                    Credit = reader.GetDecimal(1),
                                    Pass = reader.GetString(2) == "Y" ? true : false,
                                    Grade = reader.GetDecimal(3),
                                    DataYear = Int32.Parse(reader.GetString(4)) + 1911,
                                    DataSemester = Int32.Parse(reader.GetString(5)),
                                    TotalCredit = reader.IsDBNull(6) ? -1 : reader.GetDecimal(6),
                                    Average = reader.IsDBNull(7) ? -1 : reader.GetDecimal(7),
                                    ConductMark = reader.IsDBNull(8) ? -1 : reader.GetDecimal(8),
                                });
                            }
                        }
                    }
                }
                Tuple<decimal, decimal, string, decimal> tuple = Grades(course);
                cmd = "SELECT [ename], [cname], [birthday], [entrym], [degrenam], [graddat], [deptenam], [colenam] FROM [Test_ncyu_dev].[dbo].[stufile] JOIN [Test_ncyu_dev].[dbo].[sclperson] ON [Test_ncyu_dev].[dbo].[stufile].[idno] = [Test_ncyu_dev].[dbo].[sclperson].[idno] JOIN [Test_ncyu_dev].[dbo].[pubsec] ON [Test_ncyu_dev].[dbo].[stufile].[deptno] = [Test_ncyu_dev].[dbo].[pubsec].[deptno] JOIN [Test_ncyu_dev].[dbo].[pubdep] ON [Test_ncyu_dev].[dbo].[stufile].[deptno] = [Test_ncyu_dev].[dbo].[pubdep].[deptno] JOIN [Test_ncyu_dev].[dbo].[pubcol] ON [Test_ncyu_dev].[dbo].[pubcol].[colno] = [Test_ncyu_dev].[dbo].[pubdep].[colno] LEFT JOIN [Test_ncyu_dev].[dbo].[selstugracrd] ON [Test_ncyu_dev].[dbo].[stufile].[stuno] = [Test_ncyu_dev].[dbo].[selstugracrd].[stuno] WHERE [Test_ncyu_dev].[dbo].[stufile].[stuno] = '" + StudentId + "'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stu.Add(new Models.Student()
                            {
                                Name = reader.IsDBNull(0) ? reader.GetString(1) : reader.GetString(0) + " ( " + reader.GetString(1) + " )",
                                Birth = DateTime.ParseExact(TW2AD(reader.GetString(2)), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                                Enrolled = DateTime.ParseExact(TW2AD(reader.GetString(3)), "yyyyMM", CultureInfo.InvariantCulture).ToString("MMMM, yyyy", new CultureInfo("en-us")),
                                DegreeConferred = reader.IsDBNull(5) ? "blank" : reader.GetString(4),
                                DateConferred = reader.IsDBNull(5) ? "blank" : DateTime.ParseExact(TW2AD(reader.GetString(5)), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMMM, yyyy", new CultureInfo("en-us")),
                                Department = reader.GetString(6),
                                College = reader.GetString(7),
                                Issued = DateTime.Now.ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                                Score = tuple.Item1,
                                Credits = tuple.Item2,
                                Percent = tuple.Item3,
                                GPA = tuple.Item4,
                                IsRank = Isrank,
                            }); ;
                        }
                    }
                }
            }
            return Tuple.Create(stu, course);
        }
    }
}