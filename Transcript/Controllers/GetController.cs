using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using Transcript.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Transcript.Controllers
{
    public class GetController : ApiController
    {
        public IHttpActionResult Get()
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            List<Select_JSON> dept_JsonObject = new List<Select_JSON>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT[deptno], [deptnam] FROM[Test_ncyu_dev].[dbo].[pubdep]";
                
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dept_JsonObject.Add(new Select_JSON()
                            {
                                Id = reader.GetString(0).Trim(),
                                Name = reader.GetString(1).Trim(),
                            });
                        }
                    }
                }
            }
            return Ok(dept_JsonObject);
        }

        [Route("api/get/sec")]
        [HttpPost]
        public IHttpActionResult secPost([FromBody] JObject data)
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            List<Select_JSON> sec_JsonObject = new List<Select_JSON>();
            var JsonObject = JsonConvert.DeserializeObject<Sec_JSON>(data.ToString());
            string deptno = JsonObject.Id;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [secno], [secnam] FROM [Test_ncyu_dev].[dbo].[pubsec] WHERE [deptno] = '{deptno}'";

                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sec_JsonObject.Add(new Select_JSON()
                            {
                                Id = reader.GetString(0).Trim(),
                                Name = reader.GetString(1).Trim(),
                            });
                        }
                    }
                }
            }
            return Ok(sec_JsonObject);
        }

        [Route("api/get/stu")]
        [HttpPost]
        public IHttpActionResult stuPost([FromBody] JObject data)
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            List<Select_JSON> stu_JsonObject = new List<Select_JSON>();
            var JsonObject = JsonConvert.DeserializeObject<Stu_JSON>(data.ToString());
            string DeptId = JsonObject.DeptId;
            int Secno = JsonObject.Secno;
            int Grade = JsonObject.Grade;
            int Clacod = JsonObject.Clacod;
            int syearEnd = JsonObject.syearEnd;
            int semEnd = JsonObject.semEnd;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [Test_ncyu_dev].[dbo].[regstusem].[stuno], [ename] FROM [Test_ncyu_dev].[dbo].[regstusem] LEFT JOIN [Test_ncyu_dev].[dbo].[stufile] ON [Test_ncyu_dev].[dbo].[regstusem].[stuno] = [Test_ncyu_dev].[dbo].[stufile].[stuno] LEFT JOIN [Test_ncyu_dev].[dbo].[sclperson] ON [Test_ncyu_dev].[dbo].[stufile].[idno] = [Test_ncyu_dev].[dbo].[sclperson].[idno] WHERE [Test_ncyu_dev].[dbo].[regstusem].[deptno] = '{DeptId}' AND [Test_ncyu_dev].[dbo].[regstusem].[secno] = '{Secno}' AND [Test_ncyu_dev].[dbo].[regstusem].[grade] = '{Grade}' AND [Test_ncyu_dev].[dbo].[regstusem].[clacod] = '{Clacod}' AND [syear] = {syearEnd} AND [sem] = {(semEnd > 2 ? 2 : semEnd)} ORDER BY [Test_ncyu_dev].[dbo].[regstusem].[stuno]";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stu_JsonObject.Add(new Select_JSON()
                            {
                                Id = reader.GetString(0).Trim(),
                                Name = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(),
                            });
                        }
                    }
                }
            }
            return Ok(stu_JsonObject);
        }
    }
}
