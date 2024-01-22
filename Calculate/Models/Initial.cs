using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Data.SqlClient;
using System.Globalization;

namespace Calculate.Models
{
    public class Initial
    {
        public void DBWrite(SqlCommand command)
        {
            while (true)
            {
                try
                {
                    command.ExecuteNonQuery();
                    Debug.WriteLine($"db write");
                    break;
                }
                catch (SqlException ex)
                {
                    Debug.WriteLine($"db error:{ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unexpected error:{ex.Message}");
                    break;
                }
            }
        }

        public void UpdateSelstch(string user_id, int avg_cd, string StudentId, int syear, int sem, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string range = $"AND [syear] = {syear} AND [sem] = {sem}";
                if (avg_cd == 2)
                    range = $"AND (([syear] < {syear}) OR ([syear] = {syear} AND [sem] <= {sem}))";

                string cmd = $"UPDATE [selstch] SET [avg_cd] = {avg_cd}, [user_id] = '{user_id}', [updat_date] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-us"))}', [updat_time] = {DateTime.Now.ToString("HHmmss", new CultureInfo("en-us"))} WHERE [stuno] = '{StudentId}' {range}";
                SqlCommand command_update = new SqlCommand(cmd, connection);
                DBWrite(command_update);
                Debug.WriteLine(("updateselstch"));
            }
        }

        List<RegSem> GetRegSems(string StudentId, int syearEnd, int semEnd, string connectionString)
        {
            List<RegSem> regsems = new List<RegSem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //---------------------------------------學系，年級，年份和學期
                string cmd = $"SELECT [syear], [sem], [deptno], [secno], [clacod], [grade] FROM [regstusem] WHERE [stuno] = '{StudentId}' AND (([syear] < {syearEnd}) OR ([syear] = {syearEnd} AND [sem] <= {semEnd})) ORDER BY [syear], [sem]";
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
            if (semEnd == 3 && regsems.Last().sem == 2) //新增第三學期
            {
                regsems.Add(new RegSem()
                {
                    syear = regsems.Last().syear,
                    sem = 3,
                    deptno = regsems.Last().deptno,
                    secno = regsems.Last().secno,
                    clacod = regsems.Last().clacod,
                    grade = regsems.Last().grade
                });
            }
            int sem_pre = 0;
            for (int i = 0; i < regsems.Count; i++) //新增第三學期
            {
                if (sem_pre == 2 && regsems[i].sem == 1)
                {
                    regsems.Insert(i, new RegSem()
                    {
                        syear = regsems[i].syear - 1,
                        sem = 3,
                        deptno = regsems[i].deptno,
                        secno = regsems[i].secno,
                        clacod = regsems[i].clacod,
                        grade = regsems[i].grade - 1
                    });
                }
                sem_pre = regsems[i].sem;
            }
            return regsems;
        }

        void Calculate(string user_id, string DeptId, string StudentId, int Secno, int Grade, int Clacod, int syearEnd, int semEnd, bool Isrank, string connectionString)
        {
            bool rank_cd = true;
            List<RegSem> regsems = new List<RegSem>();
            if (DeptId != null) //dept
            {
                int syear = syearEnd + 1 - Grade;
                int sem = 0;
                int gradeTemp = 1;
                do
                {
                    if (sem > 2)
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

            foreach (RegSem regsem in regsems)
            {
                Debug.WriteLine((regsem.syear, regsem.sem, regsem.grade));
            }

            Selstchf selstchf = new Selstchf();

            Selstugracrd selstugracrd = new Selstugracrd();

            rank_cd = selstchf.CheckSelstchf(user_id, regsems, Isrank, connectionString) && rank_cd;

            selstugracrd.CheckSelstugracrd(user_id, regsems, Isrank, rank_cd, connectionString);
        }

        public void Init(JSON JsonObject, string connectionString)
        {
            string user_id = JsonObject.user_id;
            string DeptId = JsonObject.DeptId;
            string StudentId = JsonObject.StudentId;
            int Secno = JsonObject.Secno;
            int Grade = JsonObject.Grade;
            int Clacod = JsonObject.Clacod;
            int syearEnd = JsonObject.syearEnd;
            int semEnd = JsonObject.semEnd;
            bool Isrank = JsonObject.Isrank;
            Calculate(user_id, DeptId, StudentId, Secno, Grade, Clacod, syearEnd, semEnd, Isrank, connectionString);
        }
    }
}