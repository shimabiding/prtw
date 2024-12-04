using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using Npgsql;

namespace wp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.Run(new wrapperForm());

            while (true)
            {
                System.Console.Write("Input order_id to this line: ");
                var inputID = GetValidatedInput(System.Console.ReadLine());
                if (null == inputID) continue;
                using (var conn = CreateDatabaseConnection())
                {
                    if (null == conn) continue;
                    var result = ExecuteQueryAndGetResult(conn, inputID);
                    if (null == result) System.Console.WriteLine("Not found");
                    else UpdateUserInterface(result);
                }
            }
        }

        public static string GetValidatedInput(string s)
        {
            if (16 == s.Length)
            {
                var m = Regex.Match(s, @"^WP(\d{11})\d{3}");
                if (m.Success) return m.Groups[1].Value;
                else {
                    System.Console.WriteLine("This input is missing type");
                    return null;
                }
            }
            else
            {
                System.Console.WriteLine("This input is missing length");
                return null;
            }
        }

        public static NpgsqlConnection CreateDatabaseConnection()
        {
            var conn_str = "Server=localhost;Port=5432;Database=mng;UserID=postgres;";
            var conn = new NpgsqlConnection(conn_str);
            try { conn.Open(); }
            catch (Exception ex)
            {
                System.Console.WriteLine("Can't open a database: {0}", ex.Message);
                return null;
            }
            return conn;
        }

        public static string ExecuteQueryAndGetResult(NpgsqlConnection conn, string id)
        {
            var query = QueryWP();
            NpgsqlDataReader reader = null;
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("inputID", int.Parse(id));

                try { reader = cmd.ExecuteReader(); }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return null;
                }
            }
            if (reader.HasRows)
            {
                string prevDeptID = "";
                string prevID = "";

                while (reader.Read())
                {
                    string workStatus = reader["work_status"].ToString();
                    if ("1" == workStatus)
                    {
                        string deptID = reader["dept_id"].ToString();
                        string id = reader["order_detail_id"].ToString();

                        if (deptID == prevDeptID || string.IsNullOrEmpty(prevDeptID))
                        {
                            prevDeptID = deptID;
                            prevID = id;
                        }
                        else break;
                    }
                }
                return prevID;
            }
            else return null;
        }

        public static void UpdateUserInterface(string result)
        {
            var TARGETNAME = "beInput";
            var TARGETID = "textBox111";
            try
            {
                var targetWindow = AutomationElement.RootElement.FindFirst(
                    TreeScope.Children,
                    new PropertyCondition(AutomationElement.NameProperty, TARGETNAME)
                );
                var targetElement = targetWindow.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, TARGETID)
                );

                var vp = targetElement.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                if (null != vp)
                {
                    var f = AutomationElement.FocusedElement;
                    vp.SetValue(result);
                    f.SetFocus();
                }
            }
            catch (NullReferenceException) { System.Console.WriteLine("出力先ソフトが未起動"); }
            catch (Exception ex) { System.Console.WriteLine(ex); }
            
        }

        static string QueryWP()
        {
            var query = @"
SELECT *
FROM abcd
WHERE abcd.order_id = @inputID
    AND abcd.del_flg = 0
ORDER BY job_seq ASC
";
            return query;
        }
    }
}
