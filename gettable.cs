using System;
using System.IO;
using System.Text;
using Npgsql;

namespace Name
{
    class gettbl
    {
        static void Main(string[] args)
        {
            var conn_str = "Server=localhost;Port=5432;Database=mng;UserID=postgres;Password=;";
            var query = GenSql();

            using (var conn = new NpgsqlConnection(conn_str))
            {
                try { conn.Open(); }
                catch (Exception ex) { System.Console.WriteLine(ex.Message); }

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    NpgsqlDataReader reader = null;
                    try { reader = cmd.ExecuteReader(); }
                    catch(Exception ex) { System.Console.WriteLine(ex); }
                    
                    var result = "";
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result += reader.GetName(i) + ",";
                    }
                    result += "\n";
                    for(int i = 0; i < reader.FieldCount; i++)
                    {
                        result += reader.GetDataTypeName(i) + ",";
                    }
                    result += "\n";

                    while(reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            result += reader[i] + ",";
                        }
                        result += "\n";
                    }
                    System.Console.WriteLine(result);
                    var utf8_enc = new UTF8Encoding(false);
                    File.WriteAllText(@"result.csv", result, utf8_enc);
                }
            }
        }
        static string GenSql()
        {
            return @"
SELECT *
FROM abcd
WHERE order_id = 00000000006
    AND del_flg = 0
ORDER BY job_seq ASC
";
        }
    }
}