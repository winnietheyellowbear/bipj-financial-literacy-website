using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace bipj.Data
{
    internal static class Db
    {
        private static readonly string ConnStr =
            ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        // Non-transaction helpers
        internal static int Exec(string sql, Action<SqlParameterCollection> fill)
        {
            using (var c = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, c))
            {
                if (fill != null) fill(cmd.Parameters);
                c.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        internal static T Scalar<T>(string sql, Action<SqlParameterCollection> fill, T def = default(T))
        {
            using (var c = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, c))
            {
                if (fill != null) fill(cmd.Parameters);
                c.Open();
                var o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value)
                    ? def
                    : (T)Convert.ChangeType(o, typeof(T));
            }
        }

        internal static List<T> Query<T>(string sql, Action<SqlParameterCollection> fill, Func<SqlDataReader, T> map)
        {
            var list = new List<T>();
            using (var c = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, c))
            {
                if (fill != null) fill(cmd.Parameters);
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(map(r));
                }
            }
            return list;
        }

        // Transaction helpers
        internal static SqlConnection OpenConnection()
        {
            var c = new SqlConnection(ConnStr);
            c.Open();
            return c; // caller MUST dispose
        }

        internal static int Exec(SqlConnection c, SqlTransaction tx, string sql, Action<SqlParameterCollection> fill)
        {
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                if (fill != null) fill(cmd.Parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        internal static T Scalar<T>(SqlConnection c, SqlTransaction tx, string sql, Action<SqlParameterCollection> fill, T def = default(T))
        {
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                if (fill != null) fill(cmd.Parameters);
                var o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value)
                    ? def
                    : (T)Convert.ChangeType(o, typeof(T));
            }
        }

        internal static List<T> Query<T>(SqlConnection c, SqlTransaction tx, string sql, Action<SqlParameterCollection> fill, Func<SqlDataReader, T> map)
        {
            var list = new List<T>();
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                if (fill != null) fill(cmd.Parameters);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(map(r));
                }
            }
            return list;
        }

        internal static T QuerySingle<T>(SqlConnection c, SqlTransaction tx, string sql, Action<SqlParameterCollection> fill, Func<SqlDataReader, T> map)
            where T : class
        {
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                if (fill != null) fill(cmd.Parameters);
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return r.Read() ? map(r) : null;
                }
            }
        }
    }
}
