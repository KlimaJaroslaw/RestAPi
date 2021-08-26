using Microsoft.Data.SqlClient;
using System;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{
    static class GlobalData
    {
        public static SqlConnection connection = null;
        public static DataTable ExecSQL(string sql)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand(sql, connection);
            da.Fill(dt);
            return dt;
        }

        public static DataTable ExecCmd(SqlCommand cmd)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dt);
            return dt;
        }

        public static SqlCommand SqlIntoCommand(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql, connection);
            return cmd;
        }

        public static string DateTimeToStringWithFormat(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");

        }


        public static string DataTableToJsonObj(DataTable dt)
        {
            DataSet ds = new DataSet();
            ds.Merge(dt);
            StringBuilder JsonString = new StringBuilder();
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                JsonString.Append("[");
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    JsonString.Append("{");
                    for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                    {
                        if (j < ds.Tables[0].Columns.Count - 1)
                        {
                            JsonString.Append("\"" + ds.Tables[0].Columns[j].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[i][j].ToString() + "\",");
                        }
                        else if (j == ds.Tables[0].Columns.Count - 1)
                        {
                            JsonString.Append("\"" + ds.Tables[0].Columns[j].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == ds.Tables[0].Rows.Count - 1)
                    {
                        JsonString.Append("}");
                    }
                    else
                    {
                        JsonString.Append("},");
                    }
                }
                JsonString.Append("]");
                return JsonString.ToString();
            }
            else
            {
                return null;
            }
        }

    }
}
