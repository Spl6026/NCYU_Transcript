using FastReport;
using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Transcript.Models;
using System.Net.Http.Headers;
using System.Web.ModelBinding;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using FastReport.Utils.Json;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.IO;

namespace Transcript.Controllers
{
    public class GenerateController : ApiController
    {
        public IHttpActionResult Post([FromBody] JObject data) {
            if (data == null)
            {
                return BadRequest();
            }
            var JsonObject = JsonConvert.DeserializeObject<JSON>(data.ToString());
            string StudentId = JsonObject.StudentId;
            int syearEnd = JsonObject.syearEnd;
            int semEnd = JsonObject.semEnd;
            bool Isrank = JsonObject.Isrank;
            string path = HttpContext.Current.Server.MapPath("~");
            DataBase link = new DataBase();
            Tuple<List<Student>, List<Courses>> tuple = link.SQLGet(StudentId, syearEnd, semEnd, Isrank);
            List<Student> stu = tuple.Item1;
            List<Courses> courses = tuple.Item2;
            List<Sign> sign = new List<Sign>();
            FastReport.Utils.Config.WebMode = true;
            Report rep = new Report();

            sign.Add(new Sign(Path.Combine(path, "img")));

            rep.Load(path + "test.frx");
            rep.RegisterData(stu, "StudentRef");
            rep.RegisterData(courses, "CoursesRef");
            rep.RegisterData(sign, "SignRef");
            if (rep.Report.Prepare())
            {
                FastReport.Export.PdfSimple.PDFSimpleExport pdfExport = new FastReport.Export.PdfSimple.PDFSimpleExport();
                pdfExport.ShowProgress = true;
                pdfExport.Subject = "Transcript";
                pdfExport.Title = StudentId;
                pdfExport.Author = "NCYU";
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                rep.Report.Export(pdfExport, ms);
                rep.Dispose();
                pdfExport.Dispose();
                ms.Position = 0;
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = DateTime.Now.ToString("yyyyMMdd_") + StudentId + ".pdf";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentLength = ms.Length;
                response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                return ResponseMessage(response);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MyAction 已被執行");
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.Gone));
            }
        }
    }
}
