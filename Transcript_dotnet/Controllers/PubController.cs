using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pub.Models;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Transcript.Models;
using Microsoft.AspNetCore.Cors;
using System.Diagnostics;

namespace Pub.Controllers
{
    [EnableCors("Policy")]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PubController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public PubController(IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            configuration = config;
        }

        [HttpPost(Name = "date")]
        public IActionResult date([FromBody] Stu_JSON data)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            Verify verify = new Verify();
            string acad = data.Acadno;
            string phase= "2";
            Stu_JSON date = verify.Date(acad, phase, connectionString);
            return Ok(date);
        }

        [HttpPost(Name = "range")]
        public IActionResult range([FromBody] Range_JSON data)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            Verify verify = new Verify();
            string id;
            bool check;
            (check, id) = verify.Agent(data.user_id, data.program_no, connectionString);
            string deptno = data.deptno;
            if (data.stuno != "")
                deptno = verify.Deptno(data.stuno, data.syearEnd, data.semEnd, connectionString);

            return Ok(check ? verify.Range(id, deptno, data.tblname, data.clnname, connectionString) : false);
        }
        
        [HttpPost(Name = "check")]
        public IActionResult check([FromBody] Pub_JSON data)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            bool check = false;
            string msg;
            Verify verify = new Verify();
            if (verify.Valid(data.WebPid1, connectionString))
            {
                (check, msg) = verify.Identity(data.WebPid1, data.program_no, connectionString);
            }
            else
            {
                msg = "登入資訊已過期";
            }
            return Ok(new Check_JSON()
            {
                Ident = check,
                Msg = msg
            });
        }

        [HttpPost(Name = "acad")]
        public IActionResult acad([FromBody] Select_JSON data)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            Info info = new Info();
            Select_JSON acad = info.Acadno(data, connectionString);
            return Ok(acad);
        }

        [HttpPost(Name = "dept")]
        public IActionResult dept([FromBody] Range_JSON data)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            Verify verify = new Verify();
            string id;
            bool check;
            Debug.WriteLine(data.user_id);
            (check, id) = verify.Agent(data.user_id, data.program_no, connectionString);
            Info info = new Info();
            return Ok(check ? info.Dept_list(id, data.tblname, data.clnname, connectionString) : null);
        }

        [HttpPost(Name = "sec")]
        public IActionResult sec([FromBody] Select_JSON data)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            Info info = new Info();
            return Ok(info.Sec_list(data, connectionString));
        }

        [HttpPost(Name = "stu")]
        public IActionResult stu([FromBody] Stu_JSON data)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            Info info = new Info();
            return Ok(info.Stu_list(data, connectionString));
        }
    }
}