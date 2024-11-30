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
                System.Console.Write("Input voucher_id to this line: ");
                string inputID = GetValidatedInput(System.Console.ReadLine());
                if (null == inputID) continue;

                using (var conn = CreateDatabaseConnection())
                {
                    if (null == conn) continue;
                    var result = ExecuteQueryAndGetResult(conn, inputID);
                    if (null != result)
                    {
                        UpdateUserInterface(result, "beInput", "textBox111");
                    }
                    else
                    {
                        System.Console.WriteLine("Not found");
                    }
                }
            }
        }

        public static string GetValidatedInput(string id)
        {
            if (Regex.IsMatch(id, @"^WP\d{6}"))
            {
                return id;
            }
            else
            {
                System.Console.WriteLine("This input is missing type");
                return null;
            }
        }

        public static NpgsqlConnection CreateDatabaseConnection()
        {
            var conn_str = "Server=localhost;Port=5432;Database=mng;UserID=postgres;";
            var conn = new NpgsqlConnection(conn_str);
            try
            {
                conn.Open();
            }
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
                cmd.Parameters.AddWithValue("inputID", id);

                try
                {
                    reader = cmd.ExecuteReader();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return null;
                }
            }
            if (reader.HasRows)
            {
                string PrevID = "";
                string PrevDeptID = "";

                while (reader.Read())
                {
                    string workStatus = reader["work_status"].ToString();
                    if ("1" == workStatus)
                    {
                        string deptID = reader["dept_id"].ToString();
                        string ID = reader["order_detail_id"].ToString();

                        if (deptID == PrevDeptID || string.IsNullOrEmpty(PrevDeptID))
                        {
                            PrevDeptID = deptID;
                            PrevID = ID;
                        }
                        else break;
                    }
                }
                return PrevID;
            }
            else return null;
        }

        public static void UpdateUserInterface(string result, string name, string id)
        {
            var WINDOWNAME = name;
            var AUTOMATIONID = id;

            try
            {
                var targetWindow = AutomationElement.RootElement.FindFirst(
                    TreeScope.Children,
                    new PropertyCondition(AutomationElement.NameProperty, WINDOWNAME)
                );
                var targetElement = targetWindow.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, AUTOMATIONID)
                );

                var vp = targetElement.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                if (null != vp)
                {
                    var f = AutomationElement.FocusedElement;
                    vp.SetValue(result);
                    f.SetFocus();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        static string QueryWP()
        {
            var query = @"
SELECT *
FROM a
    INNER JOIN (
        SELECT *
        FROM b
        WHERE del_flg = 0
    ) AS b
    ON a.order_id = b.order_id
WHERE a.voucher_nid = @inputID
    AND a.order_no = 2
    AND a.del_flg = 0
ORDER BY job_seq ASC
";
            return query;
        }
    }
}