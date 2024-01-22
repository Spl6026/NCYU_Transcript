using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Transcript.Models
{
    public class DataBase
    {
        int TR = 0;

        string TW2AD(string tw)
        {
            return (int.Parse(tw.Substring(0, 3)) + 1911).ToString() + tw.Substring(3);
        }

        string Grade(Decimal grade, string pass, int drop, int arrival, bool Isgrading)
        {
            if (drop == 1)
                return "withdraw";

            if (arrival == 0)
                return "I";

            pass = pass.Trim() == "Y" ? "" : "*";
            if (Isgrading)
            {
                string grad = "";
                if (grade >= 80)
                    grad = "A";

                else if (grade >= 70)
                    grad = "B";

                else if (grade >= 60)
                    grad = "C";

                else if (grade >= 50)
                    grad = "D";

                else
                    grad = "E";

                return pass + grad;
            }
            else
                return pass + grade.ToString("0");
        }

        string TR_Count(decimal credits)
        {
            TR += (int)credits;
            return "TR";
        }

        public List<string> StuGet(string DeptId, int Secno, int Grade, int Clacod, int syearEnd, int semEnd, string connectionString)
        {
            List<string> StudentIds = new List<string>();
            string cmd = $"SELECT [stuno] FROM [regstusem] WHERE [deptno] = '{DeptId}' AND [secno] = {Secno} AND [grade] = {Grade} AND [clacod] = {Clacod} AND [syear] = {syearEnd} AND [sem] = {(semEnd > 2 ? 2 : semEnd)}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StudentIds.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return StudentIds;
        }

        public Tuple<List<Student>, List<Courses>> InfoGet(string StudentId, int syearEnd, int semEnd, bool Isrank, bool Isgrading, string connectionString)
        {
            TR = 0;
            List<Student> stu = new List<Student>();
            List<Courses> course = new List<Courses>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int acad = 0;
                string cmd = $"SELECT [acadno] FROM [stufile] WHERE [stuno] = '{StudentId}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                           acad = Int32.Parse(reader.GetString(0));
                        }
                    }
                }
                string cr = "";
                if (acad == 3 || acad == 4 || acad == 6)
                    cr = "OR [cr_class] = 4";

                cmd = $"SELECT [englishco], [credit], [pass], [totalscore], [selstch].[syear], [selstch].[sem], [rgcrd], [scoavg], [total_score], [capacity], [dropcd], [arrival_cd] FROM [selstch] LEFT JOIN [crscourse] ON [selstch].[cono] = [crscourse].[cono] LEFT JOIN [con_behavior] ON [selstch].[syear] = [con_behavior].[syear] AND [selstch].[sem] = [con_behavior].[sem] AND [selstch].[stuno] = [con_behavior].[stuno] LEFT JOIN [selstchf] ON [selstch].[syear] = [selstchf].[syear] AND [selstch].[sem] = [selstchf].[sem] AND [selstch].[stuno] = [selstchf].[stuno] WHERE [selstch].[stuno] = '{StudentId}' AND (([selstch].[syear] < {syearEnd}) OR ([selstch].[syear] = {syearEnd} AND [selstch].[sem] <= {semEnd})) UNION SELECT [englishco], [credit], CAST('Y' AS CHAR) AS [pass], NULL AS [totalscore], [crscredit].[syear], [crscredit].[sem], [rgcrd], [scoavg], [total_score], CAST('0' AS CHAR) AS [capacity], CAST('0' AS CHAR) AS [dropcd], CAST('1' AS CHAR) AS [arrival_cd] FROM [crscredit] LEFT JOIN [crscourse] ON [crscredit].[crcono] = [crscourse].[cono] LEFT JOIN [con_behavior] ON [crscredit].[syear] = [con_behavior].[syear] AND [crscredit].[sem] = [con_behavior].[sem] AND [crscredit].[stuno] = [con_behavior].[stuno] LEFT JOIN [selstchf] ON [crscredit].[syear] = [selstchf].[syear] AND [crscredit].[sem] = [selstchf].[sem] AND [crscredit].[stuno] = [selstchf].[stuno] WHERE [crscredit].[stuno] = '{StudentId}' AND (([crscredit].[syear] < {syearEnd}) OR ([crscredit].[syear] = {syearEnd} AND [crscredit].[sem] <= {semEnd})) AND ([cr_class] = 2 {cr}) ORDER BY [syear], [sem], [capacity]";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(2))
                            {
                                course.Add(new Courses()
                                {
                                    Course = reader.IsDBNull(0) ? "-" : reader.GetString(0),
                                    Credit = reader.GetDecimal(1),
                                    Grade = reader.IsDBNull(3) ? TR_Count(reader.GetDecimal(1)) : Grade(reader.GetDecimal(3), reader.GetString(2), Int32.Parse(reader.GetString(10)), Int32.Parse(reader.GetString(11)), Isgrading),
                                    DataYear = Int32.Parse(reader.GetString(4)) + 1911,
                                    DataSemester = Int32.Parse(reader.GetString(5)),
                                    TotalCredit = reader.IsDBNull(6) ? -1 : reader.GetDecimal(6),
                                    Average = reader.IsDBNull(7) ? -1 : reader.GetDecimal(7),
                                    ConductMark = reader.IsDBNull(8) ? -1 : reader.GetDecimal(8),
                                    Capacity = Int32.Parse(reader.GetString(9))
                                });
                            }
                        }
                    }
                }
                cmd = $"SELECT [ename], [cname], [birthday], [entrym], [degrenam], [graddat], [deptenam], [colenam], [scoavg], [accrgcrd], [clspgnsort], [allman], [accgpa], [selpaper].[score] FROM [stufile] LEFT JOIN [sclperson] ON [stufile].[idno] = [sclperson].[idno] LEFT JOIN [pubsec] ON [stufile].[deptno] = [pubsec].[deptno] LEFT JOIN [pubdep] ON [stufile].[deptno] = [pubdep].[deptno] LEFT JOIN [pubcol] ON [pubcol].[colno] = [pubdep].[colno] LEFT JOIN [selstugracrd] ON [stufile].[stuno] = [selstugracrd].[stuno] LEFT JOIN [selpaper] ON [stufile].[stuno] = [selpaper].[stuno] WHERE [stufile].[stuno] = '{StudentId}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal credits = 0;
                            decimal rank = 0;
                            decimal allman = 0;
                            decimal thesis = 0;
                            if (acad == 3 || acad == 4 || acad == 6) {
                                credits = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                                rank = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                                allman = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                            }
                            if(acad == 1 || acad == 2 || acad == 5)
                            {
                                thesis = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);
                            }
                            stu.Add(new Student()
                            {
                                Acad = acad,
                                Name = reader.IsDBNull(0) ? reader.GetString(1) : reader.GetString(0) + " ( " + reader.GetString(1) + " )",
                                Birth = DateTime.ParseExact(TW2AD(reader.GetString(2)), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                                Enrolled = DateTime.ParseExact(TW2AD(reader.GetString(3)), "yyyyMM", CultureInfo.InvariantCulture).ToString("MMMM, yyyy", new CultureInfo("en-us")),
                                DegreeConferred = reader.IsDBNull(5) ? "blank" : reader.GetString(4),
                                DateConferred = reader.IsDBNull(5) ? "blank" : DateTime.ParseExact(TW2AD(reader.GetString(5)), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMMM, yyyy", new CultureInfo("en-us")),
                                Department = reader.GetString(6),
                                College = reader.GetString(7),
                                Issued = DateTime.Now.ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                                Score = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                                Credits = credits,
                                Rank = rank,
                                Class = allman,
                                GPA = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12),
                                IsRank = Isrank,
                                Thesis = thesis,
                                TR = TR
                            });
                        }
                    }
                }
            }
            return Tuple.Create(stu, course);
        }
    }
}