using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Database : MonoBehaviour
{
    static string ConnStr =
         @"Server=FinalYear; Database=EduGame; Trusted_Connection=True; 
          TrustServerCertificate=True; MultipleActiveResultSets=True";

    public static async Task<List<(string id, string text, string[] answers, int correct)>>
        GetQuestions(int age, string subject, int take = 10)
    {
        var list = new List<(string, string, string[], int)>();
        using var conn = new SqlConnection(ConnStr);
        await conn.OpenAsync();
        using var cmd = new SqlCommand("dbo.GetQuestions", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Age", age);
        cmd.Parameters.AddWithValue("@Subject", subject);
        cmd.Parameters.AddWithValue("@Take", take);

        using var r = await cmd.ExecuteReaderAsync();
        string currId = null; string qText = null; int correct = 0; var ans = new string[4];
        while (await r.ReadAsync())
        {
            var qid = r.GetGuid(0).ToString();
            if (currId != null && qid != currId) { list.Add((currId, qText, ans, correct)); ans = new string[4]; }
            currId = qid; qText = r.GetString(1); correct = r.GetInt32(2);
            ans[r.GetInt32(3)] = r.GetString(4);
        }
        if (currId != null) list.Add((currId, qText, ans, correct));
        return list;
    }
}
