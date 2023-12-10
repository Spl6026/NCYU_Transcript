using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Data.SqlClient;

namespace Calculate.Models
{
    public class Selstugracrd
    {
        void RankSelstugracrd(List<Student> students, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string cmd = "";
                connection.Open();
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

        public void CheckSelstugracrd(List<RegSem> regsems, bool Isrank, bool rank_cd, string connectionString)
        {
            RegSem regsem = regsems.Last();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<Student> students = new List<Student>();
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
                    RankSelstugracrd(students, connectionString);
                }
            }
        }
    }
}