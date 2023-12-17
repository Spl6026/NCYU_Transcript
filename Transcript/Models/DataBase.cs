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

        public Tuple<List<Student>, List<Courses>> SQLGet(string StudentId, int syearEnd, int semEnd, bool Isrank, bool Isgrading, string connectionString)
        {
            TR = 0;
            List<Student> stu = new List<Student>();
            List<Courses> course = new List<Courses>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int acad = 0;
                string cmd = $"SELECT [acadno] FROM [Test_ncyu_dev].[dbo].[stufile] WHERE [stuno] = '{StudentId}'";
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
                if (acad == 3)
                    cr = "OR [cr_class] = 4";

                cmd = $"SELECT [englishco], [credit], [pass], [totalscore], [Test_ncyu_dev].[dbo].[selstch].[syear], [Test_ncyu_dev].[dbo].[selstch].[sem], [rgcrd], [scoavg], [total_score], [capacity], [dropcd], [arrival_cd] FROM [Test_ncyu_dev].[dbo].[selstch] LEFT JOIN [Test_ncyu_dev].[dbo].[crscourse] ON [Test_ncyu_dev].[dbo].[selstch].[cono] = [Test_ncyu_dev].[dbo].[crscourse].[cono] LEFT JOIN [Test_ncyu_dev].[dbo].[con_behavior] ON [Test_ncyu_dev].[dbo].[selstch].[syear] = [Test_ncyu_dev].[dbo].[con_behavior].[syear] AND [Test_ncyu_dev].[dbo].[selstch].[sem] = [Test_ncyu_dev].[dbo].[con_behavior].[sem] AND [Test_ncyu_dev].[dbo].[selstch].[stuno] = [Test_ncyu_dev].[dbo].[con_behavior].[stuno] LEFT JOIN [Test_ncyu_dev].[dbo].[selstchf] ON [Test_ncyu_dev].[dbo].[selstch].[syear] = [Test_ncyu_dev].[dbo].[selstchf].[syear] AND [Test_ncyu_dev].[dbo].[selstch].[sem] = [Test_ncyu_dev].[dbo].[selstchf].[sem] AND [Test_ncyu_dev].[dbo].[selstch].[stuno] = [Test_ncyu_dev].[dbo].[selstchf].[stuno] WHERE [Test_ncyu_dev].[dbo].[selstch].[stuno] = '{StudentId}' AND (([Test_ncyu_dev].[dbo].[selstch].[syear] < {syearEnd}) OR ([Test_ncyu_dev].[dbo].[selstch].[syear] = {syearEnd} AND [Test_ncyu_dev].[dbo].[selstch].[sem] <= {semEnd})) UNION SELECT [englishco], [credit], CAST('Y' AS CHAR) AS [pass], NULL AS [totalscore], [Test_ncyu_dev].[dbo].[crscredit].[syear], [Test_ncyu_dev].[dbo].[crscredit].[sem], [rgcrd], [scoavg], [total_score], CAST('0' AS CHAR) AS [capacity], CAST('0' AS CHAR) AS [dropcd], CAST('1' AS CHAR) AS [arrival_cd] FROM [Test_ncyu_dev].[dbo].[crscredit] LEFT JOIN [Test_ncyu_dev].[dbo].[crscourse] ON [Test_ncyu_dev].[dbo].[crscredit].[crcono] = [Test_ncyu_dev].[dbo].[crscourse].[cono] LEFT JOIN [Test_ncyu_dev].[dbo].[con_behavior] ON [Test_ncyu_dev].[dbo].[crscredit].[syear] = [Test_ncyu_dev].[dbo].[con_behavior].[syear] AND [Test_ncyu_dev].[dbo].[crscredit].[sem] = [Test_ncyu_dev].[dbo].[con_behavior].[sem] AND [Test_ncyu_dev].[dbo].[crscredit].[stuno] = [Test_ncyu_dev].[dbo].[con_behavior].[stuno] LEFT JOIN [Test_ncyu_dev].[dbo].[selstchf] ON [Test_ncyu_dev].[dbo].[crscredit].[syear] = [Test_ncyu_dev].[dbo].[selstchf].[syear] AND [Test_ncyu_dev].[dbo].[crscredit].[sem] = [Test_ncyu_dev].[dbo].[selstchf].[sem] AND [Test_ncyu_dev].[dbo].[crscredit].[stuno] = [Test_ncyu_dev].[dbo].[selstchf].[stuno] WHERE [Test_ncyu_dev].[dbo].[crscredit].[stuno] = '{StudentId}' AND (([Test_ncyu_dev].[dbo].[crscredit].[syear] < {syearEnd}) OR ([Test_ncyu_dev].[dbo].[crscredit].[syear] = {syearEnd} AND [Test_ncyu_dev].[dbo].[crscredit].[sem] <= {semEnd})) AND ([cr_class] = 2 {cr}) ORDER BY [syear], [sem], [capacity]";
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
                cmd = $"SELECT [ename], [cname], [birthday], [entrym], [degrenam], [graddat], [deptenam], [colenam], [scoavg], [accrgcrd], [clspgnsort], [allman], [accgpa], [Test_ncyu_dev].[dbo].[selpaper].[score] FROM [Test_ncyu_dev].[dbo].[stufile] LEFT JOIN [Test_ncyu_dev].[dbo].[sclperson] ON [Test_ncyu_dev].[dbo].[stufile].[idno] = [Test_ncyu_dev].[dbo].[sclperson].[idno] LEFT JOIN [Test_ncyu_dev].[dbo].[pubsec] ON [Test_ncyu_dev].[dbo].[stufile].[deptno] = [Test_ncyu_dev].[dbo].[pubsec].[deptno] LEFT JOIN [Test_ncyu_dev].[dbo].[pubdep] ON [Test_ncyu_dev].[dbo].[stufile].[deptno] = [Test_ncyu_dev].[dbo].[pubdep].[deptno] LEFT JOIN [Test_ncyu_dev].[dbo].[pubcol] ON [Test_ncyu_dev].[dbo].[pubcol].[colno] = [Test_ncyu_dev].[dbo].[pubdep].[colno] LEFT JOIN [Test_ncyu_dev].[dbo].[selstugracrd] ON [Test_ncyu_dev].[dbo].[stufile].[stuno] = [Test_ncyu_dev].[dbo].[selstugracrd].[stuno] LEFT JOIN [Test_ncyu_dev].[dbo].[selpaper] ON [Test_ncyu_dev].[dbo].[stufile].[stuno] = [Test_ncyu_dev].[dbo].[selpaper].[stuno] WHERE [Test_ncyu_dev].[dbo].[stufile].[stuno] = '{StudentId}'";
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
                            if (acad == 3) {
                                credits = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                                rank = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                                allman = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                            }
                            if(acad == 2)
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