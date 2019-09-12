using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace DBEditing
{
    public class DB_Connect
    {
        public SqlConnection Db_con = new SqlConnection(DBEditing.Properties.Settings.Default.Connection);
        SqlCommand cmd;
        SqlDataAdapter da = new SqlDataAdapter();
        public string Cur_User = "";
        public string Cur_User_Type = "";

        public void Db_State()
        {
            try
            {
                if (Db_con.State == System.Data.ConnectionState.Closed)
                {
                    Db_con.Open();
                }
            }
            catch { }
        }

        public void Non_Query(string sql)
        {
            Db_State();
            cmd = new SqlCommand(sql, Db_con);
            cmd.ExecuteNonQuery();
        }

        public void Select_DT(string sql, ref DataTable dt)
        {
            Db_State();
            dt.Rows.Clear();
            dt.Columns.Clear();
            da = new SqlDataAdapter(sql, Db_con);
            da.Fill(dt);
        }
    }
}
