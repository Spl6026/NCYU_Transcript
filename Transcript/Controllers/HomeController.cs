using FastReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        public FileResult Generate()
        {
            FastReport.Utils.Config.WebMode = true;
            Report rep = new Report();
            string path = Server.MapPath("~/test.frx");
            rep.Load(path);

            List<Models.Employee> emp = new List<Models.Employee>();
            List<Models.Employeesub> empsub = new List<Models.Employeesub>();

            emp.Add(new Models.Employee()
            {
                Name = "KUM YU HENG (甘宇恆) ",
                Birth = "June II, 1999",
                Enrolled = "September, 2018 ",
                DegreeConferred = " Bachelor of Science ",
                DateConferred = "June. 2022 ",
                Department = "Department of Computer Science and Information Engineering ",
                College = "College of Science and Engineering ",
                Issued = "November 01,2022",
            });
            int i = 0;
            for ( i = 0; i < 15; i++) {
               
                empsub.Add(new Models.Employeesub()
                {

                    sub = "Math"+i.ToString(),
                    sco = 70.ToString(),
                });
            }
            rep.RegisterData(emp, "EmployeeRef");
            rep.RegisterData(empsub, "EmployeesubRef");
            if (rep.Report.Prepare())
            {
                
                FastReport.Export.PdfSimple.PDFSimpleExport pdfExport = new FastReport.Export.PdfSimple.PDFSimpleExport();
                pdfExport.ShowProgress = true;
                pdfExport.Subject = "Subject Report"; 
                pdfExport.Title = "Report Title";
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                rep.Report.Export(pdfExport, ms);
                rep.Dispose();
                pdfExport.Dispose();
                ms.Position = 0;
                return File(ms, "application/pdf", "myreport.pdf");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MyAction 已被執行");
                return null;
            }
        }
    }
}