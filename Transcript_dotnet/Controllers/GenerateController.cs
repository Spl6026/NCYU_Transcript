using FastReport;
using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Net;
using Transcript.Models;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.IO.Compression;
using FastReport.Export.PdfSimple.PdfCore;
using System.Windows.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO.Pipes;

namespace Transcript.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GenerateController : ControllerBase
    {
        private readonly IConfiguration configuration;

        private readonly ILogger<GenerateController> _logger;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public GenerateController(IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            configuration = config;
            _webHostEnvironment = webHostEnvironment;
        }

        byte[] informGeneratePDF(Tuple<List<informStudent>, List<informCourses>> tuple, string StudentId)
        {
            string path = _webHostEnvironment.ContentRootPath;
            List<informStudent> stu = tuple.Item1;
            List<informCourses> courses = tuple.Item2;
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
                MemoryStream ms = new MemoryStream();
                rep.Report.Export(pdfExport, ms);
                rep.Dispose();
                pdfExport.Dispose();
                return ms.ToArray();
            }
            else
            {
                Debug.WriteLine("MyAction 已被執行");
                return null;
            }
        }

        byte[] GeneratePDF(Tuple<List<Student>, List<Courses>> tuple, string StudentId)
        {
            string path = _webHostEnvironment.ContentRootPath;
            List<Student> stu = tuple.Item1;
            List<Courses> courses = tuple.Item2;
            List<Sign> sign = new List<Sign>();
            FastReport.Utils.Config.WebMode = true;
            Report rep = new Report();

            sign.Add(new Sign(Path.Combine(path, "img")));

            rep.Load(path + "transcript.frx");
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
                MemoryStream ms = new MemoryStream();
                rep.Report.Export(pdfExport, ms);
                rep.Dispose();
                pdfExport.Dispose();
                return ms.ToArray();
            }
            else
            {
                Debug.WriteLine("MyAction 已被執行");
                return null;
            }
        }


        [HttpPost(Name = "inform")]
        public IActionResult inform([FromBody] Transcript_JSON data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            string StudentId = data.StudentId;
            int syearEnd = data.syearEnd;
            int semEnd = data.semEnd;

            DataBase db = new DataBase();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            Tuple<List<informStudent>, List<informCourses>> tuple = db.InformGet(StudentId, syearEnd, semEnd, connectionString);
            byte[] pdf = informGeneratePDF(tuple, StudentId);
            if (pdf == null)
                return Ok(new HttpResponseMessage(HttpStatusCode.Gone));

            string contentDisposition = $"attachment; filename={DateTime.Now.ToString("yyyyMMdd")}_{StudentId}.pdf";
            Response.Headers.Add(Microsoft.Net.Http.Headers.HeaderNames.ContentDisposition, contentDisposition);
            return File(pdf, "application/pdf");
        }


        [HttpPost(Name = "eng")]
        public IActionResult eng([FromBody] Transcript_JSON data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            string DeptId = data.DeptId;
            string StudentId = data .StudentId;
            int Secno = data.Secno;
            int Grade = data.Grade;
            int Clacod = data.Clacod;
            int syearEnd = data.syearEnd;
            int semEnd = data.semEnd;
            bool Isrank = data.Isrank;
            bool Isgrading = data.Isgrading;
            DataBase db = new DataBase();
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            if (DeptId != null) //dept
            {
                List<string> StudentIds = db.StuGet(DeptId, Secno, Grade, Clacod, syearEnd, semEnd, connectionString);
                using (var zipStream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Update))
                    {
                        foreach (string stuno in StudentIds)
                        {
                            Tuple<List<Student>, List<Courses>> tuple = db.InfoGet(stuno, syearEnd, semEnd, Isrank, Isgrading, connectionString);
                            byte[] pdf = GeneratePDF(tuple, stuno);
                            if (pdf == null)
                                return Ok(new HttpResponseMessage(HttpStatusCode.Gone));

                            var entry = zipArchive.CreateEntry($"{DateTime.Now.ToString("yyyyMMdd")}_{stuno.Trim()}.pdf");
                            using (var entryStream = entry.Open())
                            {
                                entryStream.Write(pdf, 0, pdf.Length);
                            }
                            Debug.WriteLine(stuno);
                        }
                    }
                    string contentDisposition = $"attachment; filename={DateTime.Now.ToString("yyyyMMdd")}_{DeptId}_{Secno}_{Grade}_{Clacod}.zip";
                    Response.Headers.Add(Microsoft.Net.Http.Headers.HeaderNames.ContentDisposition, contentDisposition);
                    return File(zipStream.ToArray(), "application/zip");
                }
            }
            else //stu
            {
                Tuple<List<Student>, List<Courses>> tuple = db.InfoGet(StudentId, syearEnd, semEnd, Isrank, Isgrading, connectionString);
                byte[] pdf = GeneratePDF(tuple, StudentId);
                if (pdf == null)
                    return Ok(new HttpResponseMessage(HttpStatusCode.Gone));
                string contentDisposition = $"attachment; filename={DateTime.Now.ToString("yyyyMMdd")}_{StudentId}.pdf";
                Response.Headers.Add(Microsoft.Net.Http.Headers.HeaderNames.ContentDisposition, contentDisposition);
                return File(pdf, "application/pdf");
            }
        }
    }
}
