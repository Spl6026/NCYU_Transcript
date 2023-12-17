using System.Diagnostics;
using System.Globalization;
using System;
using System.Data.SqlClient;

namespace Calculate.Models
{
    public class JSON
    {
        public string DeptId { get; set; }
        public string StudentId { get; set; }
        public int Secno { get; set; }
        public int Grade { get; set; }
        public int Clacod { get; set; }
        public int syearEnd { get; set; }
        public int semEnd { get; set; }
        public bool Isrank { get; set; }
    }

    public class Student
    {
        public string stuno { get; set; }
        public decimal scoavg { get; set; }
        public int rank { get; set; }
    }

    public class Courses
    {
        public bool Pass {  get; set; }
        public decimal Credit { get; set; }
        public decimal Grade { get; set; }
        public int dropcd { get; set; }
    }

    public class RegSem
    {
        public int syear { get; set; }
        public int sem { get; set; }
        public string deptno { get; set; }
        public int secno { get; set; }
        public int grade { get; set; }
        public int clacod { get; set; }
    }
    public class Selstch
    {
        public void UpdateSelstch(int avg_cd, string StudentId, int syear, int sem, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string range = $"AND [syear] = {syear} AND [sem] = {sem}";
                if (avg_cd == 2)
                    range = $"AND (([syear] < {syear}) OR ([syear] = {syear} AND [sem] <= {sem}))";

                string cmd = $"UPDATE [dbo].[selstch] SET [avg_cd] = {avg_cd}, [user_id] = 'test', [updat_date] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-us"))}', [updat_time] = {DateTime.Now.ToString("HHmmss", new CultureInfo("en-us"))} WHERE [stuno] = '{StudentId}' {range}";
                SqlCommand command_update = new SqlCommand(cmd, connection);
                try
                {
                    command_update.ExecuteNonQuery();
                    Debug.WriteLine("update successful");
                }
                catch (SqlException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                Debug.WriteLine(("updateselstch"));
            }
        }
    }
}