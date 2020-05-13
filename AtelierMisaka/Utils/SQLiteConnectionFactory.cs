using SQLite;
using System;
using System.IO;

namespace AtelierMisaka
{
    public class SQLiteConnectionFactory
    {
        private static readonly object locker = new object();
        private static SQLiteConnectionFactory _instance;
        public string DatabaseFile => Path.Combine(Environment.CurrentDirectory, "Data.db");

        public static SQLiteConnectionFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new SQLiteConnectionFactory();
                        }
                    }

                }
                return _instance;
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(this.DatabaseFile) { BusyTimeout = new TimeSpan(0, 0, 0, 1) };
        }

        public SQLiteConnection GetConnection(string path)
        {
            return new SQLiteConnection(path) { BusyTimeout = new TimeSpan(0, 0, 0, 1) };
        }
    }
}
