using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using Calculate.Models;
using System.IdentityModel;
using System.Globalization;
using System.Diagnostics;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text.Json.Nodes;

namespace Calculate.Controllers
{
    public class HomeController : ApiController
    {
        void RankSelstugracrd(int syear, int sem, string deptno, int secno, int grade, int clacod, string connectionString)
        {
            List<Student> students = new List<Student>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [stuno] FROM [Test_ncyu_dev].[dbo].[regstusem] WHERE [syear] = {syear} AND [sem] = {sem} AND [deptno] = '{deptno}' AND [secno] = {secno} AND [grade] = {grade} AND [clacod] = {clacod} AND [stateno] = 01";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student()
                            {
                                stuno = reader.GetString(0)
                            });
                        }
                    }
                }
                foreach (var stu in students)
                {
                    cmd = $"SELECT [scoavg] FROM [Test_ncyu_dev].[dbo].[selstugracrd] WHERE [stuno] = '{stu.stuno}'";
                    using (SqlCommand command = new SqlCommand(cmd, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stu.scoavg = reader.GetDecimal(0);
                            }
                        }
                    }
                }
                students.Sort(delegate (Student x, Student y)
                {
                    return y.scoavg.CompareTo(x.scoavg);
                });
                decimal scoavg_pre = 0;
                decimal scoavg_temp = 0;
                int rank = 0;
                int rank_cal = 1;
                int size = students.Count;
                foreach (var item in students)
                {
                    scoavg_temp = item.scoavg;
                    if (scoavg_temp == scoavg_pre)
                        rank_cal++;

                    else
                    {
                        rank += rank_cal;
                        rank_cal = 1;
                    }
                    item.rank = rank;
                    scoavg_pre = scoavg_temp;
                    cmd = $"UPDATE [dbo].[selstugracrd] SET [clspgnsort] = {item.rank}, [allman] = {size}, [user_id] = 'test', [updat_date] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-us"))}', [updat_time] = {DateTime.Now.ToString("HHmmss", new CultureInfo("en-us"))}, [rank_cd] = 1 WHERE [stuno] = '{item.stuno}'";
                    SqlCommand command_update = new SqlCommand(cmd, connection);
                    try
                    {
                        command_update.ExecuteNonQuery();
                        Debug.WriteLine("update successful");
                    }
                    catch (SqlException ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    Debug.WriteLine(("RankSelstugracrd", item.stuno, item.rank));
                }
            }
        }

        bool UpsertSelstugracrd(string StudentId, int syear, int sem, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                decimal sucrd = 0;
                decimal rgcrd = 0;
                decimal susco = 0;
                decimal GPA = 0;
                bool Insert = false;
                bool Update = false;
                bool rank_cd = false;

                string cmd = $"SELECT [sucrd], [rgcrd], [susco], [gpa] FROM [Test_ncyu_dev].[dbo].[selstchf] WHERE [stuno] = '{StudentId}' AND (([syear] < {syear}) OR ([syear] = {syear} AND [sem] <= {sem})) ORDER BY [syear], [sem]";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sucrd += reader.GetDecimal(0);
                            rgcrd += reader.GetDecimal(1);
                            susco += reader.GetDecimal(2);
                            GPA += reader.GetDecimal(0) * reader.GetDecimal(3);
                        }
                    }
                }

                decimal avg = Math.Round(susco != 0 ? susco / sucrd : 0, 2);
                GPA = Math.Round(GPA != 0 ? GPA / sucrd : 0, 2);

                cmd = $"SELECT [syear], [sem], [rank_cd] FROM [Test_ncyu_dev].[dbo].[selstugracrd] WHERE [stuno] = '{StudentId}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Insert = true;
                        }
                        else
                        {
                            rank_cd = reader.GetString(2) == "1";
                            if (!(Int32.Parse(reader.GetString(0)) == syear && Int32.Parse(reader.GetString(1)) == sem && rank_cd))
                            {
                                Update = true;
                            }
                        }
                    }
                }

                if (Insert)
                {
                    cmd = $"INSERT INTO [dbo].[selstugracrd]([stuno], [scoavg], [clspgnsort], [acadpgnsort], [deptprnsort], [allman], [syear], [sem], [user_id], [updat_date], [updat_time], [rank_cd], [accsusco], [accsucrd], [accrgcrd], [accgpa]) VALUES " +
                        $"('{StudentId}'" +
                        $",{avg}" +
                        $",NULL" +
                        $",NULL" +
                        $",NULL" +
                        $",NULL" +
                        $",{syear}" +
                        $",{sem}" +
                        $",'test'" +
                        $",'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-us"))}'" +
                        $",{DateTime.Now.ToString("HHmmss", new CultureInfo("en-us"))}" +
                        $",0" +
                        $",{susco}" +
                        $",{sucrd}" +
                        $",{rgcrd}" +
                        $",{GPA})";
                }

                if (Update)
                {
                    cmd = $"UPDATE [dbo].[selstugracrd] SET [scoavg] = {avg}, [clspgnsort] = NULL, [allman] = NULL, [syear] = {syear}, [sem] = {sem}, [user_id] = 'test', [updat_date] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-us"))}', [updat_time] = {DateTime.Now.ToString("HHmmss", new CultureInfo("en-us"))}, [rank_cd] = 0, [accsusco] = {susco}, [accsucrd] = {sucrd}, [accrgcrd] = {rgcrd}, [accgpa] = {GPA} WHERE [stuno] = '{StudentId}'";
                }
                if (Insert || Update)
                {
                    Debug.WriteLine(cmd);
                    SqlCommand command_upsert = new SqlCommand(cmd, connection);
                    try
                    {
                        command_upsert.ExecuteNonQuery();
                        Debug.WriteLine("upsert successful");
                        return false;
                    }
                    catch (SqlException ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                return true;
            }
        }

        void CheckSelstugracrd(List<RegSem> regsems, bool Isrank, string connectionString)
        {
            RegSem regsem = regsems.Last();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<Student> students = new List<Student>();
                bool rank_cd = true;

                string cmd = $"SELECT [stuno] FROM [Test_ncyu_dev].[dbo].[regstusem] WHERE [syear] = {regsem.syear} AND [sem] = {(regsem.sem > 2 ? 2 : regsem.sem)} AND [deptno] = '{regsem.deptno}' AND [secno] = {regsem.secno} AND [grade] = {regsem.grade} AND [clacod] = {regsem.clacod} AND [stateno] = 01";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student()
                            {
                                stuno = reader.GetString(0)
                            });
                        }
                    }
                }
                foreach (var stu in students)
                {
                    rank_cd = UpsertSelstugracrd(stu.stuno, regsem.syear, regsem.sem, connectionString) && rank_cd;
                }
                Debug.WriteLine(("selstugracrd_rank_cd:", rank_cd));
                if (Isrank && !rank_cd)
                {
                    RankSelstugracrd(regsem.syear, regsem.sem, regsem.deptno, regsem.secno, regsem.grade, regsem.clacod, connectionString);
                }
            }
        }

        void RankSelstchf(int syear, int sem, string deptno, int secno, int grade, int clacod, string connectionString) //----------在一個學期內，排序selstchf裡該學系所有學生的排名(不知道selstchf會不會有所有學生)
        {
            List<Student> students = new List<Student>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string cmd = $"SELECT [stuno], [scoavg] FROM [Test_ncyu_dev].[dbo].[selstchf] WHERE [syear] = {syear} AND [sem] = {sem} AND [deptno] = '{deptno}' AND [secno] = {secno} AND [grade] = {grade} AND [clacod] = {clacod} ORDER BY [scoavg] DESC";
                Debug.WriteLine(cmd);
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        decimal scoavg_pre = 0;
                        decimal scoavg_temp = 0;
                        int rank = 0;
                        int rank_cal = 1;
                        while (reader.Read())
                        {
                            scoavg_temp = reader.GetDecimal(1);
                            if (scoavg_temp == scoavg_pre)
                                rank_cal++;

                            else
                            {
                                rank += rank_cal;
                                rank_cal = 1;
                            }

                            students.Add(new Student()
                            {
                                stuno = reader.GetString(0),
                                scoavg = scoavg_temp,
                                rank = rank
                            });
                            scoavg_pre = scoavg_temp;
                        }
                    }
                }
                int size = students.Count;
                foreach (Student stu in students)
                {
                    cmd = $"UPDATE [dbo].[selstchf] SET [rank] = {stu.rank}, [allman] = {size}, [user_id] = 'test', [updat_date] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-us"))}', [updat_time] = {DateTime.Now.ToString("HHmmss", new CultureInfo("en-us"))}, [rank_cd] = 1 WHERE [stuno] = '{stu.stuno}' AND [syear] = {syear} AND [sem] = {sem} AND [deptno] = '{deptno}' AND [secno] = {secno} AND [grade] = {grade} AND [clacod] = {clacod}";
                    SqlCommand command_update = new SqlCommand(cmd, connection);
                    try
                    {
                        command_update.ExecuteNonQuery();
                        Debug.WriteLine("update successful");
                    }
                    catch (SqlException ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    Debug.WriteLine(("Rankselstchf", stu.stuno, stu.rank));
                }
            }
        }

        void InsertSelstchf(string StudentId, int syear, int sem, string deptno, int secno, int grade, int clacod, string connectionString)
        {
            List<Courses> courses = new List<Courses>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string cmd = $"SELECT [pass], [credit], [totalscore], [avg_cd] FROM [Test_ncyu_dev].[dbo].[selstch] WHERE [stuno] = '{StudentId}' AND [syear] = {syear} AND [sem] = {sem}";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                courses.Add(new Models.Courses()
                                {
                                    Credit = reader.GetDecimal(1),
                                    Grade = reader.GetDecimal(2),
                                    avg_cd = reader.GetString(3),
                                });
                            }
                        }
                    }
                }
                decimal sucrd = 0;
                decimal rgcrd = 0;
                decimal susco = 0;
                decimal GPA = 0;
                foreach (var item in courses)
                {
                    susco += item.Grade * item.Credit;

                    sucrd += item.Credit;

                    if (item.Grade >= 60)
                        rgcrd += item.Credit;

                    if (item.Grade >= 80)
                        GPA += 4 * item.Credit;

                    else if (item.Grade >= 70)
                        GPA += 3 * item.Credit;

                    else if (item.Grade >= 60)
                        GPA += 2 * item.Credit;

                    else if (item.Grade >= 50)
                        GPA += 1 * item.Credit;
                }
                cmd = $"INSERT INTO [dbo].[selstchf]([stuno], [syear], [sem], [deptno], [secno], [grade], [clacod], [sucrd], [rgcrd], [lesscd], [susco], [scoavg], [rank], [allman], [gpa], [user_id], [updat_date], [updat_time], [rank_cd]) VALUES " +
                    $"('{StudentId}'" +
                    $",{syear}" +
                    $",{sem}" +
                    $",{deptno}" +
                    $",{secno}" +
                    $",{grade}" +
                    $",{clacod}" +
                    $",{sucrd}" +
                    $",{rgcrd}" +
                    $",{(sucrd / 2 < rgcrd ? 0 : (sucrd / 3 < rgcrd ? 1 : 2))}" +
                    $",{susco}" +
                    $",{Math.Round(susco != 0 ? susco / sucrd : 0, 2)}" +
                    $",0" +
                    $",''" +
                    $",{Math.Round(GPA != 0 ? GPA / sucrd : 0, 2)}" +
                    $",'test'" +
                    $",'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-us"))}'" +
                    $",{DateTime.Now.ToString("HHmmss", new CultureInfo("en-us"))}" +
                    $",0)";
                SqlCommand command_insert = new SqlCommand(cmd, connection);
                try
                {
                    command_insert.ExecuteNonQuery();
                    Debug.WriteLine("insert successful");
                }
                catch (SqlException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        void CheckSelstchf(List<RegSem> regsems, bool Isrank, string connectionString) //做該系所Selstchf的insert&rank
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                int syear_pre = 0;
                int sem_pre = 0;
                int sem_temp = 0;
                foreach (var item in regsems)
                {
                    sem_temp = item.sem;
                    List<Student> students = new List<Student>();
                    bool rank_cd = true;

                    string cmd = $"SELECT [stuno] FROM [Test_ncyu_dev].[dbo].[regstusem] WHERE [syear] = {item.syear} AND [sem] = {item.sem} AND [deptno] = '{item.deptno}' AND [secno] = {item.secno} AND [grade] = {item.grade} AND [clacod] = {item.clacod} AND [stateno] = 01";
                    using (SqlCommand command = new SqlCommand(cmd, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                students.Add(new Student()
                                {
                                    stuno = reader.GetString(0)
                                });
                            }
                        }
                    }
                    if (sem_temp == 1 && sem_pre == 2) //第三學期
                    {
                        foreach (var stu in students)
                        {
                            bool IsExistSelstch = false;
                            bool IsExistSelstchf = false;
                            cmd = $"SELECT [stuno] FROM [Test_ncyu_dev].[dbo].[selstch] WHERE [stuno] = '{stu.stuno}' AND [syear] = {syear_pre} AND [sem] = 3";
                            using (SqlCommand command = new SqlCommand(cmd, connection))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        IsExistSelstch = true;
                                    }
                                }
                            }
                            cmd = $"SELECT [stuno] FROM [Test_ncyu_dev].[dbo].[selstchf] WHERE [stuno] = '{stu.stuno}' AND [syear] = {syear_pre} AND [sem] = 3";
                            using (SqlCommand command = new SqlCommand(cmd, connection))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        IsExistSelstchf = true;
                                    }
                                }
                            }
                            if (IsExistSelstch && !IsExistSelstchf)
                            {
                                InsertSelstchf(stu.stuno, syear_pre, 3, item.deptno, item.secno, item.grade, item.clacod, connectionString);
                            }
                        }
                    }
                    foreach (var stu in students)
                    {
                        cmd = $"SELECT [rank_cd] FROM [Test_ncyu_dev].[dbo].[selstchf] WHERE [stuno] = '{stu.stuno}' AND [syear] = {item.syear} AND [sem] = {item.sem}";
                        using (SqlCommand command = new SqlCommand(cmd, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    InsertSelstchf(stu.stuno, item.syear, item.sem, item.deptno, item.secno, item.grade, item.clacod, connectionString);
                                    rank_cd = false;
                                }
                                else
                                {
                                    rank_cd = reader.GetString(0) == "1" && rank_cd;
                                }
                            }
                        }
                    }
                    Debug.WriteLine(("selstchf_rank_cd:", rank_cd));
                    if (Isrank && !rank_cd)
                    {
                        RankSelstchf(item.syear, item.sem, item.deptno, item.secno, item.grade, item.clacod, connectionString);
                    }
                    syear_pre = item.syear;
                    sem_pre = item.sem;
                }
            }
        }

        List<RegSem> GetRegSems(string StudentId, int syearEnd, int semEnd, string connectionString)
        {
            List<RegSem> regsems = new List<RegSem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //---------------------------------------學系，年級，年份和學期
                string cmd = $"SELECT [syear], [sem], [deptno], [secno], [clacod], [grade] FROM [Test_ncyu_dev].[dbo].[regstusem] WHERE stuno = '{StudentId}' AND (([syear] < {syearEnd}) OR ([syear] = {syearEnd} AND [sem] <= {semEnd})) ORDER BY [syear], [sem]";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            regsems.Add(new RegSem()
                            {
                                syear = Int32.Parse(reader.GetString(0)),
                                sem = Int32.Parse(reader.GetString(1)),
                                deptno = reader.GetString(2),
                                secno = Int32.Parse(reader.GetString(3)),
                                clacod = Int32.Parse(reader.GetString(4)),
                                grade = Int32.Parse(reader.GetString(5))
                            });
                        }
                    }
                }
            }
            return regsems;
        }

        IHttpActionResult Calculate(string DeptId, string StudentId, int Secno, int Grade, int Clacod, int syearEnd, int semEnd, bool Isrank, string connectionString)
        {
            List<RegSem> regsems = new List<RegSem>();
            if (DeptId != null) //dept
            {
                int syear = syearEnd + 1 - Grade;
                int sem = 1;
                int gradeTemp = 1;
                do
                {
                    if(sem > 2)
                    {
                        syear++;
                        gradeTemp++;
                        sem = 1;
                    }
                    else
                        sem++;
                    regsems.Add(new RegSem()
                    {
                        syear = syear,
                        sem = sem,
                        deptno = DeptId,
                        secno = Secno,
                        clacod = Clacod,
                        grade = gradeTemp
                    });
                } while (!(syear == syearEnd && sem == semEnd));
            }
            else
            {
                regsems = GetRegSems(StudentId, syearEnd, semEnd, connectionString);//---------------------------------------取deptno, secno, clacod, grade, syear, sem
            }

            CheckSelstchf(regsems, Isrank, connectionString);

            CheckSelstugracrd(regsems, Isrank, connectionString);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return ResponseMessage(response);
        }

        public IHttpActionResult Post([FromBody] JObject data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            var JsonObject = JsonConvert.DeserializeObject<JSON>(data.ToString());
            string DeptId = JsonObject.DeptId;
            string StudentId = JsonObject.StudentId;
            int Secno = JsonObject.Secno;
            int Grade = JsonObject.Grade;
            int Clacod = JsonObject.Clacod;
            int syearEnd = JsonObject.syearEnd;
            int semEnd = JsonObject.semEnd;
            bool Isrank = JsonObject.Isrank;
            string connectionString = "Data Source=SPL\\SQLEXPRESS;Initial Catalog=Test_ncyu_dev;Persist Security Info=True;User ID=ccadmsup;Password=ccap2dev98";
            return Calculate(DeptId, StudentId, Secno, Grade, Clacod, syearEnd, semEnd, Isrank, connectionString);
        }
    }
}