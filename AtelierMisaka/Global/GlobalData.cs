using AtelierMisaka.Models;
using AtelierMisaka.ViewModels;
using AtelierMisaka.Views;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AtelierMisaka
{
    public class GlobalData
    {
        public static VM_Main VM_MA = null;
        public static VM_Download VM_DL = null;
        public static SynchronizationContext SyContext = null;
        public static Downloader DownLP = null;
        public static bool? CheckResult = false;
        public static SiteType CurrentSite = SiteType.Fanbox;

        public static int CurrentCulStr = 1;

        public static DateTime StartTime = DateTime.Now;

        public static CLastDateDic LastDateDic = null;
        public static DownloadLogList DLLogs = null;

        public static Pop_Setting Pop_Setting = null;
        public static Pop_Document Pop_Document = null;

        public static Dictionary<string, int> RetryCounter = new Dictionary<string, int>();

        public static Dictionary<string, string> UrlProvider = new Dictionary<string, string>()
        {
            { "youtube", "https://www.youtube.com/watch?v=" },
            { "fanbox", "https://www.fanbox.cc/" },
            { "gist", "https://gist.github.com/" },
            { "soundcloud", "https://soundcloud.com/" },
            { "vimeo", "https://vimeo.com/" },
            { "twitter", "https://twitter.com/i/web/status/" },
            { "google_forms", "https://docs.google.com/forms/d/e/" },
        };

        private static Lazy<FanboxUtils> _utilFanbox = new Lazy<FanboxUtils>();
        private static Lazy<FantiaUtils> _utilFantia = new Lazy<FantiaUtils>();
        private static Lazy<PatreonUtils> _utilPatreon = new Lazy<PatreonUtils>();

        public static BaseUtils CaptureUtil
        {
            get
            {
                switch (VM_MA.Site)
                {
                    case SiteType.Fanbox:
                        return _utilFanbox.Value;
                    case SiteType.Fantia:
                        return _utilFantia.Value;
                    default:
                        return _utilPatreon.Value;
                }
            }
        }

        public static FantiaUtils FantiaRetryUtil
        {
            get => _utilFantia.Value;
        }
    }
}
