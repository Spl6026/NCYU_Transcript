using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Calculate.Models;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Calculate.Controllers
{
    public class CalculateController : ApiController
    {
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
                if(sem_pre == 2 && regsems[i].sem == 1)
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

        IHttpActionResult Calculate(string DeptId, string StudentId, int Secno, int Grade, int Clacod, int syearEnd, int semEnd, bool Isrank, string connectionString)
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

            foreach (RegSem regsem in regsems)
            {
                Debug.WriteLine((regsem.syear, regsem.sem, regsem.grade));
            }

            Selstchf selstchf = new Selstchf();

            Selstugracrd selstugracrd = new Selstugracrd();

            rank_cd = selstchf.CheckSelstchf(regsems, Isrank, connectionString) && rank_cd;

            selstugracrd.CheckSelstugracrd(regsems, Isrank, rank_cd, connectionString);

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
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            return Calculate(DeptId, StudentId, Secno, Grade, Clacod, syearEnd, semEnd, Isrank, connectionString);
        }
    }
}