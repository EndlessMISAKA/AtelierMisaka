using AtelierMisaka.Commands;
using AtelierMisaka.Models;
using AtelierMisaka.ViewModels;
using AtelierMisaka.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public class GlobalData
    {
        public static VM_Main VM_MA = null;
        public static VM_Download VM_DL = null;
        public static SynchronizationContext SyContext = null;
        public static Dictionary<string, List<DLSP>> DownloadLogs = null;
        public static DB_Layer Dbl = new DB_Layer();
        public static Downloader DownLP = null;

        private static Pop_Setting _pop_Setting = null;
        private static Pop_Document _pop_Document = null;
        private static string _invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        public static ParamCommand<BackType> BackCommand = new ParamCommand<BackType>((flag) =>
        {
            if (flag == BackType.Main)
            {
                VM_MA.PopPage = _pop_Setting;
                VM_MA.LZindex = 3;
            }
            else
            {
                VM_MA.PopPage = null;
                VM_MA.IsShowDocument = false;
                VM_MA.LZindex = 0;
            }
        });

        public static ParamCommand<BaseItem> ShowDocumentCommand = new ParamCommand<BaseItem>((bi) =>
        {
            VM_MA.SelectedDocument = bi;
            VM_MA.IsShowDocument = true;
            _pop_Document.LoadData(bi);
            VM_MA.PopPage = _pop_Document;
            VM_MA.LZindex = 3;
        });

        public static ParamCommand<BaseItem> GetCoverCommand = new ParamCommand<BaseItem>(async(bi) =>
        {
            VM_MA.SelectedDocument = bi;
            if (string.IsNullOrEmpty(bi.CoverPic))
            {
                return;
            }
            bi.NeedLoadCover = false;
            bool flag = false;
            BaseUtils bu = (VM_MA.Site == SiteType.Fanbox) ? new FanboxUtils() : null;
            await Task.Run(() =>
            {
                flag = bu.GetCover(bi);
            });
            if (!flag)
            {
                bi.NeedLoadCover = true;
            }
        });

        public static CommonCommand ShowDLCommand = new CommonCommand(() =>
        {
            DownLP.Show();
        });

        public static CommonCommand OpenLinkCommand = new CommonCommand(() =>
        {
            System.Diagnostics.Process.Start(VM_MA.SelectedDocument.Link);
        });

        public static ParamCommand<string> OpenBrowserCommand = new ParamCommand<string>((link) =>
        {
            System.Diagnostics.Process.Start(link);
        });

        public static ParamCommand<object[]> DownloadCommand = new ParamCommand<object[]>((args) =>
        {
            BaseItem bi = (BaseItem)args[1];
            int index = (int)args[2];
            string sp = $"{VM_DL.SavePath}\\{VM_MA.Artist.AName}\\{bi.CreateDate.ToString("yyyyMMdd_HHmm")}_${bi.Fee}_{(bi.Title)}";
            Directory.CreateDirectory(sp);
            if (!Directory.Exists(sp))
            {
                sp = ReplacePath(sp);
                Directory.CreateDirectory(sp);
            }
            DownloadItem di = null;
            if ((bool)args[0])
            {
                di = new DownloadItem
                {
                    FileName = bi.FileNames[index],
                    Link = bi.ContentUrls[index],
                    SavePath = sp,
                    CTime = bi.CreateDate,
                    SourceDocu = bi
                };
            }
            else
            {
                di = new DownloadItem
                {
                    FileName = bi.MediaNames[index],
                    Link = bi.MediaUrls[index],
                    SavePath = sp,
                    CTime = bi.CreateDate,
                    SourceDocu = bi
                };
            }
            VM_DL.DownLoadItemList.Add(di);
            ShowDLCommand.Execute(null);
        });

        public static void Init()
        {
            _pop_Setting = new Pop_Setting();
            _pop_Document = new Pop_Document();
            SyContext = SynchronizationContext.Current;
        }

        public static bool OverPayment(int feeRequired)
        {
            return (feeRequired < VM_MA.Artist.PayLowInt) || (VM_MA.Artist.PayHighInt != -1 && (VM_MA.Artist.PayHighInt < feeRequired));
        }

        public static bool OverTime(BaseItem bi)
        {
            return VM_MA.UseDate && (bi.CreateDate <= VM_MA.LastDate || bi.UpdateDate <= VM_MA.LastDate);
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
            if (ais.Count > 0 && ais.Last().AName != "自定义")
            {
                ais.Add(new ArtistInfo());
            }
            return ais;
        }

        public static string ReplacePath(string path)
        {
            foreach (char c in _invalid)
            {
                path = path.Replace(c.ToString(), "_");
            }

            return path;
        }

        public static string RemoveLastDot(string path)
        {
            Match ma = new Regex(@"\.+$").Match(path);
            if (ma.Success)
            {
                path = path.Substring(0, path.Length - ma.Groups[0].Value.Length);
            }
            return path;
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

    public enum BackType
    {
        Main,
        Pop
    }

    public enum SiteType
    {
        Fanbox,
        Patreon
    }

    public enum ErrorType
    {
        Web,
        IO,
        UnKnown,
        NoError
    }

    public enum DownloadStatus
    {
        Waiting,
        Downloading,
        Paused,
        Completed,
        Error,
        Cancel,
        Common,
        Null
    }
}
