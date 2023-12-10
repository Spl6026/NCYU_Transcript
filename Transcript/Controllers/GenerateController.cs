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
using System.Data.SqlClient;
using System.Web.Http.Results;
using System.Diagnostics;
using System.IO.Compression;
using FastReport.Export.PdfSimple.PdfCore;
using System.Windows.Forms;

namespace Transcript.Controllers
{
    public class GenerateController : ApiController
    {
        byte[] GeneratePDF(Tuple<List<Student>, List<Courses>> tuple, string StudentId)
        {
            string path = HttpContext.Current.Server.MapPath("~");
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

        public IHttpActionResult Post([FromBody] JObject data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            var JsonObject = JsonConvert.DeserializeObject<Transcript_JSON>(data.ToString());
            string DeptId = JsonObject.DeptId;
            string StudentId = JsonObject.StudentId;
            int Secno = JsonObject.Secno;
            int Grade = JsonObject.Grade;
            int Clacod = JsonObject.Clacod;
            int syearEnd = JsonObject.syearEnd;
            int semEnd = JsonObject.semEnd;
            bool Isrank = JsonObject.Isrank;
            List<string> StudentIds = new List<string>();
            DataBase link = new DataBase();
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;

            if (DeptId != null) //dept
            {
                string cmd = $"SELECT [stuno] FROM [Test_ncyu_dev].[dbo].[regstusem] WHERE [deptno] = '{DeptId}' AND [secno] = {Secno} AND [grade] = {Grade} AND [clacod] = {Clacod} AND [syear] = {syearEnd} AND [sem] = {(semEnd > 2 ? 2 : semEnd)}";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(cmd, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                StudentIds.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                using (var zipStream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Update))
                    {
                        foreach (string stuno in StudentIds)
                        {
                            Tuple<List<Student>, List<Courses>> tuple = link.SQLGet(stuno, syearEnd, semEnd, Isrank, connectionString);
                            byte[] pdf = GeneratePDF(tuple, stuno);
                            if (pdf == null)
                                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.Gone));

                            var entry = zipArchive.CreateEntry($"{DateTime.Now.ToString("yyyyMMdd")}_{stuno.Trim()}.pdf");
                            using (var entryStream = entry.Open())
                            {
                                entryStream.Write(pdf, 0, pdf.Length);
                            }
                            Debug.WriteLine(stuno);
                        }
                    }
                    string path = HttpContext.Current.Server.MapPath("~");
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(zipStream.ToArray());
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = $"{DateTime.Now.ToString("yyyyMMdd")}_{DeptId}_{Secno}_{Grade}_{Clacod}.zip";
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                    response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    return ResponseMessage(response);
                }
            }
            else //stu
            {
                Tuple<List<Student>, List<Courses>> tuple = link.SQLGet(StudentId, syearEnd, semEnd, Isrank, connectionString);
                byte[] pdf = GeneratePDF(tuple, StudentId);
                if (pdf == null)
                    return ResponseMessage(new HttpResponseMessage(HttpStatusCode.Gone));

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(pdf);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = $"{DateTime.Now.ToString("yyyyMMdd")}_{StudentId}.pdf";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentLength = pdf.Length;
                response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                return ResponseMessage(response);
            }
        }
    }
}
