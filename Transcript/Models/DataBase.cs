using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.SqlClient;

namespace Transcript.Models
{
    public class DataBase
    {
        string TW2AD(string tw)
        {
            return (int.Parse(tw.Substring(0, 3)) + 1911).ToString() + tw.Substring(3);
        }

        public Tuple<List<Student>, List<Courses>> SQLGet(string StudentId, int syearEnd, int semEnd, bool Isrank, string connectionString)
        {
            List<Student> stu = new List<Student>();
            List<Courses> course = new List<Courses>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string cmd = $"SELECT [englishco], [credit], [pass], [totalscore], [Test_ncyu_dev].[dbo].[selstch].[syear], [Test_ncyu_dev].[dbo].[selstch].[sem], [rgcrd], [scoavg], [total_score] FROM [Test_ncyu_dev].[dbo].[selstch] JOIN [Test_ncyu_dev].[dbo].[crscourse] ON [Test_ncyu_dev].[dbo].[selstch].[cono] = [Test_ncyu_dev].[dbo].[crscourse].[cono] LEFT JOIN [Test_ncyu_dev].[dbo].[con_behavior] ON [Test_ncyu_dev].[dbo].[selstch].[syear] = [Test_ncyu_dev].[dbo].[con_behavior].[syear] AND [Test_ncyu_dev].[dbo].[selstch].[sem] = [Test_ncyu_dev].[dbo].[con_behavior].[sem] AND [Test_ncyu_dev].[dbo].[selstch].[stuno] = [Test_ncyu_dev].[dbo].[con_behavior].[stuno]  LEFT JOIN [Test_ncyu_dev].[dbo].[selstchf] ON [Test_ncyu_dev].[dbo].[selstch].[syear] = [Test_ncyu_dev].[dbo].[selstchf].[syear] AND [Test_ncyu_dev].[dbo].[selstch].[sem] = [Test_ncyu_dev].[dbo].[selstchf].[sem] AND [Test_ncyu_dev].[dbo].[selstch].[stuno] = [Test_ncyu_dev].[dbo].[selstchf].[stuno] WHERE [Test_ncyu_dev].[dbo].[selstch].[stuno] = '{StudentId}' AND (([Test_ncyu_dev].[dbo].[selstch].[syear] < {syearEnd}) OR ([Test_ncyu_dev].[dbo].[selstch].[syear] = {syearEnd} AND [Test_ncyu_dev].[dbo].[selstch].[sem] <= {semEnd})) ORDER BY [syear], [sem]";
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
                cmd = $"SELECT [ename], [cname], [birthday], [entrym], [degrenam], [graddat], [deptenam], [colenam], [scoavg], [accrgcrd], [clspgnsort], [allman], [accgpa] FROM [Test_ncyu_dev].[dbo].[stufile] JOIN [Test_ncyu_dev].[dbo].[sclperson] ON [Test_ncyu_dev].[dbo].[stufile].[idno] = [Test_ncyu_dev].[dbo].[sclperson].[idno] JOIN [Test_ncyu_dev].[dbo].[pubsec] ON [Test_ncyu_dev].[dbo].[stufile].[deptno] = [Test_ncyu_dev].[dbo].[pubsec].[deptno] JOIN [Test_ncyu_dev].[dbo].[pubdep] ON [Test_ncyu_dev].[dbo].[stufile].[deptno] = [Test_ncyu_dev].[dbo].[pubdep].[deptno] JOIN [Test_ncyu_dev].[dbo].[pubcol] ON [Test_ncyu_dev].[dbo].[pubcol].[colno] = [Test_ncyu_dev].[dbo].[pubdep].[colno] LEFT JOIN [Test_ncyu_dev].[dbo].[selstugracrd] ON [Test_ncyu_dev].[dbo].[stufile].[stuno] = [Test_ncyu_dev].[dbo].[selstugracrd].[stuno] WHERE [Test_ncyu_dev].[dbo].[stufile].[stuno] = '{StudentId}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stu.Add(new Student()
                            {
                                Name = reader.IsDBNull(0) ? reader.GetString(1) : reader.GetString(0) + " ( " + reader.GetString(1) + " )",
                                Birth = DateTime.ParseExact(TW2AD(reader.GetString(2)), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                                Enrolled = DateTime.ParseExact(TW2AD(reader.GetString(3)), "yyyyMM", CultureInfo.InvariantCulture).ToString("MMMM, yyyy", new CultureInfo("en-us")),
                                DegreeConferred = reader.IsDBNull(5) ? "blank" : reader.GetString(4),
                                DateConferred = reader.IsDBNull(5) ? "blank" : DateTime.ParseExact(TW2AD(reader.GetString(5)), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMMM, yyyy", new CultureInfo("en-us")),
                                Department = reader.GetString(6),
                                College = reader.GetString(7),
                                Issued = DateTime.Now.ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                                Score = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                                Credits = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9),
                                Rank = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10),
                                Class = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11),
                                GPA = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12),
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