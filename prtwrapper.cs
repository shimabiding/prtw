using Npgsql;
using System.Data;

namespace wrapper{

class program{
    static void Main(string[] args){
        var query = GenSqlID();
        var conn_str = "Server=localhost;Port=5432;Database=postgres;UserID=postgres;";

		using(var conn = new NpgsqlConnection(conn_str)){
			conn.Open();
            using(var cmd = new NpgsqlCommand(query, conn)){
                var res = cmd.ExecuteReader();

				while(res.Read()){
            		System.Console.Write(res["job_seq"]+", ");
            		System.Console.Write(res["deliv"]+", ");
            		System.Console.Write(res["mngcode"]+", ");
            		System.Console.WriteLine(res["subj"]);
        		}
				System.Console.ReadLine();
            }
        }
        
        
    }

    static string GenSqlID() {
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
	WHERE orderid = 123417
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

}