using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace bipj.Data
{
    internal static class Db
    {
        static readonly string ConnStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        internal static int Exec(string sql, Action<SqlParameterCollection> fill)
        {
            using (var c = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, c))
            {
                fill(cmd.Parameters);
                c.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        internal static T Scalar<T>(string sql, Action<SqlParameterCollection> fill, T def = default)
        {
            using (var c = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, c))
            {
                fill(cmd.Parameters);
                c.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? def : (T)Convert.ChangeType(o, typeof(T));
            }
        }

        internal static List<T> Query<T>(string sql, Action<SqlParameterCollection> fill, Func<SqlDataReader, T> map)
        {
            var list = new List<T>();
            using (var c = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, c))
            {
                fill(cmd.Parameters);
                c.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(map(r));
            }
            return list;
        }
    }
}