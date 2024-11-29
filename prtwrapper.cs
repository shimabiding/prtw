using Npgsql;
using System.Data;
using System;
using System.Windows.Automation;

namespace wrapper
{

    class program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                System.Console.Write("Input detail_id on this line: "); //WP231006
                var id = System.Console.ReadLine();
                if (id.Length != 8)
                {
                    System.Console.WriteLine("Missing length");
                    continue; //長さが8以外のときidのReadLineまで戻る
                }
                var query = GenSqlID();
                var conn_str = "Server=localhost;Port=5432;Database=mng;UserID=postgres;";

                using (var conn = new NpgsqlConnection(conn_str))
                {
                    try
                    { conn.Open(); }
                    catch (Exception ex)
                    { System.Console.WriteLine(ex.Message); }

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var _c = cmd.Parameters.AddWithValue("inputid", id);
                        NpgsqlDataReader res = null;

                        try
                        {
                            res = cmd.ExecuteReader();
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine(e.Message);
                            continue;
                        }

                        if (!res.HasRows) System.Console.WriteLine("not found");
                        else
                        {
                            var o = "";
                            var obefore = "";
                            var work_status = "";
                            var dept_id = "";
                            var dept_idbefore = "";

                            System.Console.WriteLine("DB returned {0} result what is SELECT", res.HasRows);
                            while (res.Read())
                            {
                                o = res["order_detail_id"].ToString();
                                work_status = res["work_status"].ToString();

                                if (work_status == "1")
                                {
                                    dept_id = res["dept_id"].ToString();
                                    if (dept_idbefore == dept_id || dept_idbefore == "")
                                    {
                                        dept_idbefore = dept_id;
                                        obefore = o;
                                        continue;
                                    }
                                    else { break; }
                                }
                            }

                            var resid = obefore;
                            System.Console.WriteLine(resid);

                            var aui = new AutomationUI();
                            try
                            {
                                AutomationElement tw = aui.FindWindowByName("beInput");
                                if (tw == null) return;
                                AutomationElement te = aui.FindElementByAutomationId(tw, "textBox111");
                                if (te == null) return;
                                aui.SetTextToElement(te, resid);
                            }
                            catch (Exception ex)
                            {
                                System.Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        static string GenSqlID()
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
WHERE a.voucher_nid = @inputid
    AND a.order_no = 2
    AND a.del_flg = 0
ORDER BY job_seq ASC
";
            return query;
        }
    }

    class AutomationUI
    {
        public AutomationElement FindWindowByName(string name)
        {
            return AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, name));
        }

        public AutomationElement FindElementByAutomationId(AutomationElement parent, string id)
        {
            return parent.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, id));
        }

        public void SetTextToElement(AutomationElement element, string text)
        {
            try
            {
                var vp = element.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                if (vp == null) return;
                var f = AutomationElement.FocusedElement; //SetValue先のウィンドウがアクティブウィンドウになってしまうので、今のウィンドウ（コンソール）のElementを保持
                vp.SetValue(text);
                f.SetFocus(); //SetValueしたウィンドウからフォーカスを奪いコンソールをアクティブにする
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

}
