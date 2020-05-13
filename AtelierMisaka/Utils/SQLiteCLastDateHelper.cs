using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierMisaka
{
    public class SQLiteCLastDateHelper
    {
        private SQLiteConnectionFactory _factory = SQLiteConnectionFactory.Instance;

        private static readonly object locker = new object();
        private static SQLiteCLastDateHelper _instance;
        public static SQLiteCLastDateHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new SQLiteCLastDateHelper();
                        }
                    }

                }
                return _instance;
            }
        }

        public SQLiteCLastDateHelper()
        {
            _factory.GetConnection().CreateTable<CLastDate>();
        }

        public Dictionary<SiteType, List<CLastDate>> GetAll()
        {
            try
            {
                using (var conn = _factory.GetConnection())
                {
                    return conn.Table<CLastDate>().GroupBy(x => x.Site).ToDictionary(x => x.Key, x => x.ToList());
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int InsertDate(CLastDate cld)
        {
            try
            {
                using (var conn = _factory.GetConnection())
                {
                    return conn.Insert(cld);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int UpdateDate(CLastDate cld)
        {
            try
            {
                using (var conn = _factory.GetConnection())
                {
                    return conn.Update(cld);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
