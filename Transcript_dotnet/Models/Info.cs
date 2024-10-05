using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Pub.Models
{
    public class Info
    {
        public Select_JSON Acadno(Select_JSON JsonObject, string connectionString)
        {
            string stuno = JsonObject.Id;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [acadno] FROM [stufile] WHERE [stuno] = '{stuno}'";

                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                           return new Select_JSON()
                            {
                                Id = reader.GetString(0).Trim(),
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public List<Select_JSON> Dept_list(string id, string tblname, string clnname, string connectionString)
        {
            List<Select_JSON> dept_JsonObject = new List<Select_JSON>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [pubdep].[deptno], [pubdep].[deptnam] FROM [EMPDATASCTY], [pubdep] WHERE [EMPDATASCTY].[TBLNAME] = '{tblname}' AND [EMPDATASCTY].[CLNNAME] = '{clnname}' AND ([pubdep].[deptno] BETWEEN [EMPDATASCTY].[DATASCRP1] AND [EMPDATASCTY].[DATASCRP2]) AND [EMPDATASCTY].[EMP_NO] = '{id}' ORDER BY [pubdep].[deptno]";

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
            return dept_JsonObject;
        }

        public List<Select_JSON> Sec_list(Select_JSON JsonObject, string connectionString)
        {
            string deptno = JsonObject.Id;
            List<Select_JSON> sec_JsonObject = new List<Select_JSON>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [secno], [secnam] FROM [pubsec] WHERE [deptno] = '{deptno}'";

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
            return sec_JsonObject;
        }

        public List<Select_JSON> Stu_list(Stu_JSON JsonObject, string connectionString)
        {
            string DeptId = JsonObject.DeptId;
            int Secno = JsonObject.Secno;
            int Grade = JsonObject.Grade;
            int Clacod = JsonObject.Clacod;
            int syearEnd = JsonObject.syearEnd;
            int semEnd = JsonObject.semEnd;
            List<Select_JSON> stu_JsonObject = new List<Select_JSON>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [regstusem].[stuno], [ename] FROM [regstusem] LEFT JOIN [stufile] ON [regstusem].[stuno] = [stufile].[stuno] LEFT JOIN [sclperson] ON [stufile].[idno] = [sclperson].[idno] WHERE [regstusem].[deptno] = '{DeptId}' AND [regstusem].[secno] = '{Secno}' AND [regstusem].[grade] = '{Grade}' AND [regstusem].[clacod] = '{Clacod}' AND [syear] = {syearEnd} AND [sem] = {(semEnd > 2 ? 2 : semEnd)} ORDER BY [regstusem].[stuno]";
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
            return stu_JsonObject;
        }
    }
}