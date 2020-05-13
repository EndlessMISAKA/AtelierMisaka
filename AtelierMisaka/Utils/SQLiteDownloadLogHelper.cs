using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public class SQLiteDownloadLogHelper
    {
        private SQLiteConnectionFactory _factory = SQLiteConnectionFactory.Instance;

        private static readonly object locker = new object();
        private static SQLiteDownloadLogHelper _instance;
        public static SQLiteDownloadLogHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new SQLiteDownloadLogHelper();
                        }
                    }

                }
                return _instance;
            }
        }

        public SQLiteDownloadLogHelper()
        {
            _factory.GetConnection().CreateTable<DownloadLog>();
        }

        public List<DownloadLog> GetLogs(string cid, SiteType st)
        {
            try
            {
                using (var conn = _factory.GetConnection())
                {
                    return conn.Table<DownloadLog>().Where(v => v.Site == st && v.CId == cid).ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int InsertLog(DownloadLog dl)
        {
            try
            {
                using (var conn = _factory.GetConnection())
                {
                    return conn.Insert(dl);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> InsertLogs(IEnumerable<DownloadLog> dls)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = _factory.GetConnection())
                    {
                        return conn.InsertAll(dls);
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            });
        }
    }
}
