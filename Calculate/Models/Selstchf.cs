using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Linq;

namespace Calculate.Models
{
    public class Selstchf
    {
        void RankSelstchf(int syear, int sem, string deptno, int secno, int grade, int clacod, string connectionString) //----------在一個學期內，排序selstchf裡該學系所有學生的排名
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

        void InsertSelstchf(string StudentId, int syear, int sem, string deptno, int secno, int grade, int clacod, int acadno, string connectionString)
        {
            List<Courses> courses = new List<Courses>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [pass], [credit], [totalscore], [dropcd], [capacity] FROM [Test_ncyu_dev].[dbo].[selstch] WHERE [stuno] = '{StudentId}' AND [syear] = {syear} AND [sem] = {sem} AND [arrival_cd] = '1'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                if (!((Int32.Parse(reader.GetString(4)) == 2 || Int32.Parse(reader.GetString(4)) == 3) && acadno != 3))
                                {
                                    courses.Add(new Courses()
                                    {
                                        Pass = reader.GetString(0).Trim() == "Y" ? true : false,
                                        Credit = reader.GetDecimal(1),
                                        Grade = reader.GetDecimal(2),
                                        dropcd = Int32.Parse(reader.GetString(3)),
                                    });
                                }
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
                    if (item.dropcd != 1)
                    {
                        susco += item.Grade * item.Credit;

                        sucrd += item.Credit;

                        if (item.Pass)
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
                    Selstch selstch = new Selstch();
                    selstch.UpdateSelstch(1, StudentId, syear, sem, connectionString);
                }
                catch (SqlException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public bool CheckSelstchf(List<RegSem> regsems, bool Isrank, string connectionString) //做該系所Selstchf的insert&rank
        {
            bool rank_cd_all = true;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                List<Student> students = new List<Student>();
                RegSem regsem = regsems.Last();
                int acadno = 0;
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

                cmd = $"SELECT [acadno] FROM [Test_ncyu_dev].[dbo].[pubdep] WHERE [deptno] = '{regsem.deptno}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            acadno = Int32.Parse(reader.GetString(0));
                        }
                    }
                }

                foreach (var item in regsems)
                {
                    
                    bool rank_cd = true;
                    
                    if (item.sem == 3) //第三學期
                    {
                        foreach (var stu in students)
                        {
                            bool IsExistSelstch = false;
                            bool IsExistSelstchf = false;
                            cmd = $"SELECT [stuno] FROM [Test_ncyu_dev].[dbo].[selstch] WHERE [stuno] = '{stu.stuno}' AND [syear] = {item.syear} AND [sem] = 3";
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
                            cmd = $"SELECT [stuno] FROM [Test_ncyu_dev].[dbo].[selstchf] WHERE [stuno] = '{stu.stuno}' AND [syear] = {item.syear} AND [sem] = 3";
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
                                InsertSelstchf(stu.stuno, item.syear, 3, item.deptno, item.secno, item.grade, item.clacod, acadno, connectionString);
                            }
                        }
                    }
                    else
                    {
                        foreach (var stu in students)
                        {
                            cmd = $"SELECT [rank_cd] FROM [Test_ncyu_dev].[dbo].[selstchf] WHERE [stuno] = '{stu.stuno}' AND [syear] = {item.syear} AND [sem] = {item.sem}";
                            using (SqlCommand command = new SqlCommand(cmd, connection))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        InsertSelstchf(stu.stuno, item.syear, item.sem, item.deptno, item.secno, item.grade, item.clacod, acadno, connectionString);
                                        rank_cd = false;
                                    }
                                    else
                                    {
                                        rank_cd = reader.GetString(0) == "1" && rank_cd;
                                    }
                                }
                            }
                        }
                    }
                    Debug.WriteLine(("selstchf_rank_cd:", rank_cd));
                    rank_cd_all = rank_cd_all && rank_cd;
                    if ((Isrank && !rank_cd) && item.sem != 3)
                    {
                        RankSelstchf(item.syear, item.sem, item.deptno, item.secno, item.grade, item.clacod, connectionString);
                    }
                }
            }
            return rank_cd_all;
        }
    }
}