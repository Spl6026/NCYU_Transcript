using FastReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public static string TW2AD(string tw)
        {
            string TwYear = tw.Substring(0, 3);
            string Date = tw.Substring(3);
            string ad = (int.Parse(TwYear) + 1911).ToString() + Date;
            return ad;
        }
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SQLGet(FormCollection form)
        {
            string StudentId = form["id"];
            List<Models.Student> stu = new List<Models.Student>();
            string connectionString = "Data Source=SPL\\SQLEXPRESS;Initial Catalog=Test_ncyu_dev;Persist Security Info=True;User ID=ccadmsup;Password=ccap2dev98";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = "SELECT [ename], [cname], [birthday], [entrym], [degrenam], [graddat], [deptenam], [colenam] FROM [Test_ncyu_dev].[dbo].[stufile] JOIN [Test_ncyu_dev].[dbo].[sclperson] ON [Test_ncyu_dev].[dbo].[stufile].[idno] = [Test_ncyu_dev].[dbo].[sclperson].[idno] JOIN [Test_ncyu_dev].[dbo].[pubsec] ON [Test_ncyu_dev].[dbo].[stufile].[deptno] = [Test_ncyu_dev].[dbo].[pubsec].[deptno] JOIN [Test_ncyu_dev].[dbo].[pubdep] ON [Test_ncyu_dev].[dbo].[stufile].[deptno] = [Test_ncyu_dev].[dbo].[pubdep].[deptno] JOIN [Test_ncyu_dev].[dbo].[pubcol] ON [Test_ncyu_dev].[dbo].[pubcol].[colno] = [Test_ncyu_dev].[dbo].[pubdep].[colno] WHERE [stuno] = '" + StudentId + "'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stu.Add(new Models.Student()
                            {
                                Name = reader.IsDBNull(0) ? reader.GetString(1) : reader.GetString(0) + " ( " + reader.GetString(1) + " )",
                                Birth = DateTime.ParseExact(TW2AD(reader.GetString(2)), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                                Enrolled = DateTime.ParseExact(TW2AD(reader.GetString(3)), "yyyyMM", CultureInfo.InvariantCulture).ToString("MMMM, yyyy", new CultureInfo("en-us")),
                                DegreeConferred = reader.IsDBNull(5) ? null : reader.GetString(4),
                                DateConferred = reader.IsDBNull(5) ? null : DateTime.ParseExact(TW2AD(reader.GetString(5)), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMMM, yyyy", new CultureInfo("en-us")),
                                Department = reader.GetString(6),
                                College = reader.GetString(7),
                                Issued = DateTime.Now.ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                            });
                        }
                    }
                }
            }
            return Generate(stu, StudentId);
        }

        public FileResult Generate(List<Models.Student> stu, string StudentId)
        {
            FastReport.Utils.Config.WebMode = true;
            Report rep = new Report();
            string path = Server.MapPath("~/test.frx");
            rep.Load(path);

            List<Models.Courses> course = new List<Models.Courses>();

            int i = 0;
            for ( i = 0; i < 10; i++) {

                course.Add(new Models.Courses()
                {
                    CourseId = i.ToString(),
                    Course = "Math" + i.ToString(),
                    Credit = 2,
                    Grade = 70,
                    DataYear = 2023,
                    DataSemester = 1,
                });
            }
            for (; i < 20; i++) {
                course.Add(new Models.Courses()
                {
                    CourseId = i.ToString(),
                    Course = "Math" + i.ToString(),
                    Credit = 2,
                    Grade = 70,
                    DataYear = 2023,
                    DataSemester = 2,
                });
            }
            rep.RegisterData(stu, "StudentRef");
            rep.RegisterData(course, "CoursesRef");
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
                return File(ms, "application/pdf", DateTime.Now.ToString("yyyyMMdd_") + StudentId + ".pdf");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MyAction 已被執行");
                return null;
            }
        }
    }
}