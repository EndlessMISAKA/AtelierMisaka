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
        public static OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Data.mdb;Persist Security Info=False;");

        static OleDbDataAdapter oda = new OleDbDataAdapter();
        static OleDbCommand cmd;
        static DataSet myds;

        public static DataSet getDS(string strSQL)
        {
            try
            {
                conn.Open();
                myds = new DataSet();
                oda = new OleDbDataAdapter(strSQL, conn);
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

        public static int setDS(string strSQL)
        {
            int n = 0;
            try
            {
                conn.Open();
                cmd = new OleDbCommand(strSQL, conn);
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
        private StringBuilder sql = new StringBuilder();
        private DataSet ds = null;
        
        public void OpenConn()
        {
            Db_Base.conn.Open();
        }

        public void CloseConn()
        {
            Db_Base.conn.Close();
        }


    }
}
