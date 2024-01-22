using System;
using System.Collections.Generic;
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
        public IHttpActionResult Post([FromBody] JObject data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            var JsonObject = JsonConvert.DeserializeObject<JSON>(data.ToString());
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;
            Initial initial = new Initial();
            initial.Init(JsonObject, connectionString);
            return Ok();
        }
    }
}