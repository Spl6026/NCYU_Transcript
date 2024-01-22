using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Pub.Models;
using System.IO;

namespace Pub.Controllers
{
    public class PubController : ApiController
    {
        [System.Web.Http.Route("api/pub/range")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult rangePost([FromBody] JObject data)
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            var JsonObject = JsonConvert.DeserializeObject<Range_JSON>(data.ToString());
            Verify verify = new Verify();
            string id;
            bool check;
            (check, id) = verify.Agent(JsonObject.user_id, JsonObject.program_no, connectionString);
            return Ok(check ? verify.Range(id, JsonObject.data, JsonObject.tblname, JsonObject.clnname, connectionString) : false);
        }

        [System.Web.Http.Route("api/pub/check")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult checkPost([FromBody] JObject data)
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            var JsonObject = JsonConvert.DeserializeObject<Pub_JSON>(data.ToString());
            bool check = false;
            string msg;
            Verify verify = new Verify();
            if (verify.Valid(JsonObject.WebPid1, connectionString))
            {
                (check, msg) = verify.Identity(JsonObject.WebPid1, JsonObject.program_no, connectionString);
            }
            else
            {
                msg = "登入資訊已過期";
            }

            return Ok(new Check_JSON()
            {
                Check = check,
                Msg = msg
            });
        }

        [System.Web.Http.Route("api/pub/dept")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult deptPost([FromBody] JObject data)
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            var JsonObject = JsonConvert.DeserializeObject<Range_JSON>(data.ToString());
            Verify verify = new Verify();
            string id;
            bool check;
            (check, id) = verify.Agent(JsonObject.user_id, JsonObject.program_no, connectionString);
            Info info = new Info();
            return Ok(check ? info.Dept_list(id, JsonObject.tblname, JsonObject.clnname, connectionString) : null);
        }

        [System.Web.Http.Route("api/pub/sec")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult secPost([FromBody] JObject data)
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            var JsonObject = JsonConvert.DeserializeObject<Select_JSON>(data.ToString());
            Info info = new Info();
            return Ok(info.Sec_list(JsonObject, connectionString));
        }

        [System.Web.Http.Route("api/pub/stu")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult stuPost([FromBody] JObject data)
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            var JsonObject = JsonConvert.DeserializeObject<Stu_JSON>(data.ToString());
            Info info = new Info();
            return Ok(info.Stu_list(JsonObject, connectionString));
        }


        
    }
}