using AtelierMisaka.Models;
using AtelierMisaka.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AtelierMisaka
{
    public class GlobalMethord
    {
        private static readonly string _invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        
        public static void Init()
        {
            GlobalData.Pop_Setting = new Pop_Setting();
            GlobalData.Pop_Document = new Pop_Document();
            GlobalData.SyContext = SynchronizationContext.Current;
            GlobalData.DLLogs = new DownloadLogList();
            GlobalData.LastDateDic = new CLastDateDic();
        }

        public static void UpdateCulture()
        {
            if (null != GlobalData.VM_MA)
            {
                GlobalData.VM_MA.UpdateCul = true;
            }
            if (null != GlobalData.VM_DL)
            {
                GlobalData.VM_DL.UpdateCul = true;
            }
        }

        public static void ErrorLog(string msg)
        {
            try
            {
                File.AppendAllText("error.log", "------------------" + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + msg + Environment.NewLine);
            }
            catch { }
        }

        public static void ErrorDownload(string msg, DownloadItem di)
        {
            try
            {
                StringBuilder sb = new StringBuilder("------------" + Environment.NewLine);
                sb.Append(DateTime.Now.ToString() + Environment.NewLine);
                sb.Append($"{di.SourceDocu.Title}:{di.SourceDocu.Link}{Environment.NewLine}");
                sb.Append($"{di.FileName}{Environment.NewLine}");
                sb.Append(msg + Environment.NewLine);
                File.AppendAllText("error_download.log", sb.ToString());
            }
            catch { }
        }

        public static void ExportErrorDownload(DownloadItem di)
        {
            StringBuilder sb = new StringBuilder("------------" + Environment.NewLine);
            sb.Append(DateTime.Now.ToString() + Environment.NewLine);
            sb.Append($"{di.SourceDocu.Title}:{di.SourceDocu.Link}{Environment.NewLine}");
            sb.Append($"{di.FileName}{Environment.NewLine}");
            File.AppendAllText("Export_Errordownload.txt", sb.ToString());
        }

        public static bool OverPayment(int feeRequired)
        {
            return (feeRequired < GlobalData.VM_MA.Artist.PayLowInt) || (GlobalData.VM_MA.Artist.PayHighInt != -1 && (GlobalData.VM_MA.Artist.PayHighInt < feeRequired));
        }

        public static bool OverTime(DateTime dt)
        {
            if (GlobalData.VM_MA.UseDate)
            {
                if (dt < GlobalData.VM_MA.LastDate)
                {
                    return true;
                }
                else if (!string.IsNullOrEmpty(GlobalData.VM_MA.Date_End))
                {
                    return dt > GlobalData.VM_MA.LastDate_End;
                }
            }
            return false;
        }

        public static bool IsFileExist(string folder, string name)
        {
            if (GlobalData.VM_MA.CheckFile)
            {
                var fp = Path.Combine(folder, name);
                return File.Exists(fp);
            }
            return true;
        }

        public static void SaveNorSet()
        {
            try
            {
                File.WriteAllText("Settings\\Setting_Normal.ini", GlobalData.VM_MA.CheckFile ? "1" : "0");
            }
            catch (Exception ex)
            {
                GlobalData.VM_MA.Messages = ex.Message;
            }
        }

        public static string ConverToJson(IEnumerable<ArtistInfo> ais)
        {
            return JsonConvert.SerializeObject(ais);
        }

        public static List<ArtistInfo> ReadArtists(string jsonp)
        {
            List<ArtistInfo> ais = new List<ArtistInfo>() { new ArtistInfo() };
            if (File.Exists(jsonp))
            {
                try
                {
                    ais = JsonConvert.DeserializeObject<List<ArtistInfo>>(File.ReadAllText(jsonp));
                }
                catch { }
            }
            if (ais.Count > 0 && !string.IsNullOrEmpty(ais.Last().Id))
            {
                ais.Add(new ArtistInfo());
            }
            return ais;
        }

        public static string ReplacePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            path = path.Trim();
            foreach (char c in _invalid)
            {
                path = path.Replace(c.ToString(), "_");
            }
            return path;
        }

        public static string RemoveLastDot(string path)
        {
            Match ma = GlobalRegex.GetRegex(RegexType.RemoveLastDot).Match(path);
            if (ma.Success)
            {
                path = path.Substring(0, path.Length - ma.Groups[0].Value.Length);
            }
            return path.Trim();
        }
        
        public static string RemoveAllDot(string title)
        {
            title = title.Replace(".", "");
            if (title.Length > 80)
            {
                title = title.Substring(0, 80);
            }
            return title.Trim();
        }



        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern void ILFree(IntPtr pidlList);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags);

        public static void ExplorerFile(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                return;

            if (Directory.Exists(filePath))
                System.Diagnostics.Process.Start(@"explorer.exe", "/select,\"" + filePath + "\"");
            else
            {
                IntPtr pidlList = ILCreateFromPathW(filePath);
                if (pidlList != IntPtr.Zero)
                {
                    try
                    {
                        Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                    }
                    finally
                    {
                        ILFree(pidlList);
                    }
                }
            }
        }
    }
}
