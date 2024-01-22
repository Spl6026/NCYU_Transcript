using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;

namespace Pub.Models
{
    public class Verify
    {
        public bool Valid(string WebPid1, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT count(*) FROM [pubwebpid] WHERE [pid] = '{WebPid1}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0) == 1 ? true : false;
                        }
                        else 
                        { 
                            return false; 
                        }
                    }
                }
            }
        }

        public bool Range(string id, string data, string tblname, string clnname, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT * FROM [EMPDATASCTY] WHERE [EMP_NO] = '{id}' AND [TBLNAME] = '{tblname}' AND [CLNNAME] = '{clnname}' AND ('{data}' BETWEEN [DATASCRP1] AND [DATASCRP2])";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read();
                    }
                }
            }
        }

        public (bool, string) Agent(string id, string program_no, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string agent_no = "";
                string cmd = $"SELECT [s_run], [agent_no], [actbdatetime], [actedatetime] FROM [webpersecurity] WHERE [id] = '{id}' AND [program_no] = '{program_no}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.GetString(0) == "0")
                            {
                                return (false, "權限不足");
                            }

                            if (!reader.IsDBNull(1))
                            {
                                agent_no = reader.GetString(1);
                                DateTime begin = DateTime.Parse(reader.GetString(2));
                                DateTime end = DateTime.Parse(reader.GetString(3));
                                if (DateTime.Compare(begin, DateTime.Now) == 1 || DateTime.Compare(DateTime.Now, end) == 1)
                                {
                                    return (false, "非代理期間");
                                }
                            }
                            else
                            {
                                return (true, id); //有權限 沒代理人
                            }
                        }
                        else
                        {
                            return (false, "沒有權限資料");
                        }
                    }
                }
                return (true, agent_no);
            }
        }

        (bool, string) Check(string id, string program_no, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [s_run] FROM [webpersecurity] WHERE [id] = '{id}' AND [program_no] = '{program_no}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0) == "1" ? (true, id) : (false, "權限不足");
                        }
                        else
                        {
                            return (false, "沒有權限資料");
                        }
                    }
                }
            }
        }

        public (bool, string) Identity(string WebPid1, string program_no, string connectionString)
        {
            string id = "";
            string emp_group = "";
            string prog_group = "";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string cmd = $"SELECT [id], [emp_group] FROM [pubwebpid] WHERE [pid] = '{WebPid1}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = reader.GetString(0);
                            emp_group = reader.GetString(1);
                        }
                        else
                        {
                            return (false, "沒這個人");
                        }
                    }
                }

                cmd = $"SELECT [run_yn], [isctltime], [actbdatetime], [actedatetime], [progfilter] FROM [webprogram] WHERE [program_no] = '{program_no}'";
                using (SqlCommand command = new SqlCommand(cmd, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.GetString(0) == "0")
                            {
                                return (false, "關閉中");
                            }
                            if (reader.GetString(1) == "1")
                            {
                                DateTime begin = DateTime.Parse(reader.GetString(2));
                                DateTime end = DateTime.Parse(reader.GetString(3));
                                if (DateTime.Compare(begin, DateTime.Now) == 1 || DateTime.Compare(DateTime.Now, end) == 1)
                                {
                                    return (false, "非開放期間");
                                }
                            }
                            prog_group = reader.GetString(4);
                        }
                        else
                        {
                            return (false, "沒找到程式");
                        }
                    }
                }
            }
            if (prog_group == "7")
            {
                if (emp_group == "7")
                {
                    //(b)
                    return Check(id, program_no, connectionString);
                }
                else
                {
                    //(c)
                    bool check = false;
                    string agent_no = "";
                    string msg = "";
                    (check, agent_no) = Agent(id, program_no, connectionString);
                    (check, msg) = Check(agent_no, program_no, connectionString);
                    if (check)
                    {
                        return (true, id);
                    }
                    else
                    {
                        return (false, msg);
                    }
                }
            }
            else
            {
                return prog_group == emp_group ? (true, id) : (false, "身分不符合");
            }
        }
    }
}