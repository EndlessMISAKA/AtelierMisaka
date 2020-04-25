using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka
{

    public class Db_Base
    {
        public static OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;Persist Security Info=False;");

        //static OleDbDataAdapter oda = new OleDbDataAdapter();
        //static OleDbCommand cmd;
        //static DataSet myds;

        public static DataSet GetDS(string strSQL)
        {
            DataSet myds = null;
            try
            {
                conn.Open();
                myds = new DataSet();
                OleDbDataAdapter oda = new OleDbDataAdapter(strSQL, conn);
                oda.Fill(myds);
                oda.Dispose();
            }
            catch
            { }
            finally
            {
                conn.Close();
            }
            return myds;
        }

        public static int SetDS(string strSQL)
        {
            int n = 0;
            try
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand(strSQL, conn);
                n = cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch
            { }
            finally
            {
                conn.Close();
            }
            return n;
        }
    }

    public class DB_Layer
    {
        //private StringBuilder sql = new StringBuilder();
        private DataSet ds = null;
        
        public void OpenConn()
        {
            Db_Base.conn.Open();
        }

        public void CloseConn()
        {
            Db_Base.conn.Close();
        }

        public Dictionary<string, List<DLSP>> GetLog(string cid)
        {
            Dictionary<string, List<DLSP>> result = new Dictionary<string, List<DLSP>>();
            string sql = $"SELECT * FROM DLLogs WHERE CID='{cid}'";
            ds = Db_Base.GetDS(sql);
            if (null != ds)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        var ro = ds.Tables[0].Rows[i];
                        var pid = ro["PID"].ToString();
                        if (result.ContainsKey(pid))
                        {
                            result[pid].Add(new DLSP() { Savepath = ro["SavePath"].ToString(), Link = ro["DLink"].ToString() });
                        }
                        else
                        {
                            result.Add(pid, new List<DLSP>() { new DLSP() { Savepath = ro["SavePath"].ToString(), Link = ro["DLink"].ToString() } });
                        }
                    }
                }
                ds.Dispose();
            }
            return result;
        }

        public bool InsertLog(string cid, string pid, string savep, string link)
        {
            string sql = $"INSERT INTO DLLogs(CID,PID,SavePath,DLink) VALUES('{cid}','{pid}','{savep}','{link}')";
            int re = Db_Base.SetDS(sql);
            return re == 1;
        }

        public bool InsertDate(string cid, DateTime dt)
        {
            string sql = $"INSERT INTO CLast VALUES('{cid}', #{dt.ToString()}#)";
            int re = Db_Base.SetDS(sql);
            return re == 1;
        }

        public bool UpdateDate(string cid, DateTime dt)
        {
            string sql = $"UPDATE CLast SET LastDate=#{dt.ToString()}# WHERE CID='{cid}'";
            int re = Db_Base.SetDS(sql);
            return re == 1;
        }

        public bool? GetLastDate(string cid, out DateTime dt)
        {
            string sql = $"SELECT * FROM CLast WHERE CID='{cid}'";
            ds = Db_Base.GetDS(sql);
            dt = DateTime.MinValue;
            if (null != ds)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (DateTime.TryParse(ds.Tables[0].Rows[0]["LastDate"].ToString(), out DateTime dt1))
                    {
                        dt = dt1;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                ds.Dispose();
            }
            return null;
        }
    }
}
