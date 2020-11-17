using System;
using System.IO;
using System.Threading;

namespace AtelierMisaka_FileManager
{
    public class GlobalData
    {
        public static SynchronizationContext SyContext = null;
        public static bool? CheckResult = false;

        public static int CurrentCulStr = 0;


        public static void ErrorLog(string msg)
        {
            try
            {
                File.AppendAllText("error.log", "------------------" + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + msg + Environment.NewLine);
            }
            catch { }
        }
    }
}
