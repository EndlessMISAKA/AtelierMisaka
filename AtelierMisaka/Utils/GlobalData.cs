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

        private static Lazy<FanboxUtils> _utilFanbox = new Lazy<FanboxUtils>();
        private static Lazy<FantiaUtils> _utilFantia = new Lazy<FantiaUtils>();

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

        public static ParamCommand<BaseItem> GetCoverCommand = new ParamCommand<BaseItem>(async (bi) =>
        {
            VM_MA.SelectedDocument = bi;
            if (string.IsNullOrEmpty(bi.CoverPic))
            {
                return;
            }
            bi.NeedLoadCover = false;
            //bool flag = false;
            BaseUtils bu = GetUtils();
            bu.GetCover(bi);
            //await Task.Run(() =>
            //{
            //    flag = bu.GetCover(bi);
            //});
            //if (!flag)
            //{
            //    bi.NeedLoadCover = true;
            //}
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

        public static CommonCommand LikePostCommand = new CommonCommand(async () =>
        {
            if (VM_MA.IsLiked_Document)
                return;
            ErrorType et = ErrorType.NoError;
            await Task.Run(() =>
            {
                et = GetUtils().LikePost(VM_MA.SelectedDocument.ID, VM_MA.Artist.Cid);
            });
            if (et == ErrorType.NoError)
            {
                VM_MA.IsLiked_Document = true;
            }
            else
            {
                if (VM_MA.IsLiked_Document)
                    return;

                switch (et)
                {
                    case ErrorType.Security:
                        VM_MA.Messages = $"认证机制出错{Environment.NewLine}请联系开发者";
                        break;
                    case ErrorType.Web:
                        VM_MA.Messages = $"网络错误，请重试";
                        break;
                    default:
                        VM_MA.Messages = $"尚未支持此站点";
                        break;
                }
            }
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

        public static BaseUtils GetUtils()
        {
            switch (VM_MA.Site)
            {
                case SiteType.Fanbox:
                    return _utilFanbox.Value;
                case SiteType.Fantia:
                    return _utilFantia.Value;
                default:
                    return null;
            }
        }

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

        public static bool OverTime(DateTime updt)
        {
            return VM_MA.UseDate && updt <= VM_MA.LastDate;
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
        Fantia,
        Patreon
    }

    public enum ErrorType
    {
        Web,
        IO,
        Cookies,
        Path,
        Security,

        UnKnown,
        NoError,
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
