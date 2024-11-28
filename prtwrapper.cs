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
                System.Console.Write("Input detail_id on this line: ");
                var id = System.Console.ReadLine();
                if (id.Length != 6)
                {
                    System.Console.WriteLine("Missing length");
                    continue; //長さが6以外のときidのReadLineまで戻る
                }
                var query = GenSqlID(id); //123417
                var conn_str = "Server=localhost;Port=5432;Database=postgres;UserID=postgres;";

                using (var conn = new NpgsqlConnection(conn_str))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var m = "";
                        var j = "";
                        NpgsqlDataReader res = null;

                        try
                        {
                            System.Console.WriteLine(id);
                            res = cmd.ExecuteReader();
                            System.Console.WriteLine("Execute後");
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine(e.Message);
                            continue; //存在しない
                        }

                        System.Console.WriteLine(res.HasRows);
                        if (!res.HasRows) System.Console.WriteLine("not found");
                        else
                        {
                            System.Console.WriteLine("DB returned {0} result what is SELECT", res.HasRows);
                            while (res.Read())
                            {
                                m = res["mngcode"].ToString();
                                j = res["job_seq"].ToString();

                                System.Console.Write(m + ", ");
                                System.Console.Write(j + ", ");
                                System.Console.Write(res["deliv"] + ", ");
                                System.Console.WriteLine(res["subj"]);
                            }

                            var inputText = m + "-" + j;

                            System.Console.WriteLine(inputText);

                            var aui = new AutomationUI();
                            try
                            {
                                AutomationElement tw = aui.FindWindowByName("beInput");
                                if (tw == null) return;
                                AutomationElement te = aui.FindElementByAutomationId(tw, "textBox111");
                                if (te == null) return;
                                aui.SetTextToElement(te, inputText);
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

        static string GenSqlID(string id)
        {
            var query = @"
SELECT
    a.mngcode,
    a.subnum,
    b.subj,
    c.C_amount,
    c.deliv,
    c.job_seq,
    d.cus_name
FROM (
    SELECT *
    FROM a
    WHERE orderid = "
    + id
    + @"
    )a

INNER JOIN (
    SELECT *
    FROM b
    )b
    ON a.bid = b.bid

INNER JOIN (
    SELECT *
    FROM c
    )c
    ON a.bid = c.bid

INNER JOIN (
    SELECT *
    FROM d
    )d
    ON b.cusid = d.cusid
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
