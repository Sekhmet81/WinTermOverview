using Npgsql;
using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace WinTermOverview
{
    public class DatabaseObjectClass
    {
        public NpgsqlConnectionStringBuilder Connection;

        public DatabaseObjectClass(string databaseName, string ipAddress, string passWord)
        {
            Connection = new NpgsqlConnectionStringBuilder
            {
                Password = passWord,
                Username = "postgres",
                Database = databaseName,
                Host = ipAddress,
                Port = 5432
            };
        }
        public DataSet ReturnDataset(string sql)
        {
            DataSet ds = new DataSet();
            NpgsqlDataAdapter da;
            using (NpgsqlConnection pgConn = new NpgsqlConnection(Connection.ConnectionString))
            {
                try
                {
                    pgConn.Open();
                    da = new NpgsqlDataAdapter(sql, pgConn);
                    ds = new DataSet();
                    da.Fill(ds);

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }// end sqlconection
            return ds;
        }
    }
}
