using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AtelierMisaka
{
    public class GlobalRegex
    {
        private static string _regex_RemoveLastDot = string.Empty;
        private static string _regex_ProxyString = string.Empty;

        private static string _regex_PatreonCid = string.Empty;
        private static string _regex_PatreonEmail = string.Empty;
        private static string _regex_PatreonHtmlImg = string.Empty;

        private static string _regex_FanboxUrl1 = string.Empty;
        private static string _regex_FanboxUrl2 = string.Empty;
        private static string _regex_FanboxCSRF = string.Empty;

        private static string _regex_FantiaIdName = string.Empty;
        private static string _regex_FantiaPlan = string.Empty;
        private static string _regex_FantiaPostId = string.Empty;
        private static string _regex_FantiaUrl = string.Empty;

        private static Lazy<Regex> _re_RemoveLastDot = null;
        private static Lazy<Regex> _re_ProxyString = null;

        public static void Initialize()
        {
            if (!Directory.Exists("Settings"))
            {
                Directory.CreateDirectory("Settings");
            }
            if (File.Exists("Settings\\RegexStr.ini"))
            {
                string[] regexs = File.ReadAllLines("Settings\\RegexStr.ini");
                _regex_RemoveLastDot = regexs[0];
                _regex_ProxyString = regexs[1];

                _regex_PatreonCid = regexs[2];
                _regex_PatreonEmail = regexs[3];
                _regex_PatreonHtmlImg = regexs[4];

                _regex_FanboxUrl1 = regexs[5];
                _regex_FanboxUrl2 = regexs[6];
                _regex_FanboxCSRF = regexs[7];

                _regex_FantiaIdName = regexs[8];
                _regex_FantiaPlan = regexs[9];
                _regex_FantiaPostId = regexs[10];
                _regex_FantiaUrl = regexs[11];
            }
            else
            {
                _regex_RemoveLastDot = @"\.+$";
                _regex_ProxyString = @"^\d+\.\d+\.\d+\.\d+:\d+$";

                _regex_PatreonCid = @"self"": ""https://www.patreon.com/api/campaigns/(\d+)";
                _regex_PatreonEmail = @"email"": ""(.+?)""";
                _regex_PatreonHtmlImg = @"<p><img.+?></p>";

                _regex_FanboxUrl1 = @"^https://www\.fanbox\.cc/@(.+?)$";
                _regex_FanboxUrl2 = @"^https://(.+?)\.fanbox\.cc/?$";
                _regex_FanboxCSRF = "csrfToken\":\"(\\w+)\"";

                _regex_FantiaIdName = @"/fanclubs/(\d+)""><strong>(.+?)</strong>";
                _regex_FantiaPlan = @"\((.+?)円/月\)</strong";
                _regex_FantiaPostId = @"block"" href=""/posts/(\d+)";
                _regex_FantiaUrl = @"^https://fantia.jp/fanclubs/(\d+)$";

                File.AppendAllLines("Settings\\RegexStr.ini", new string[] 
                {
                    _regex_RemoveLastDot,
                    _regex_ProxyString,

                    _regex_PatreonCid,
                    _regex_PatreonEmail,
                    _regex_PatreonHtmlImg,

                    _regex_FanboxUrl1,
                    _regex_FanboxUrl2,
                    _regex_FanboxCSRF,

                    _regex_FantiaIdName,
                    _regex_FantiaPlan,
                    _regex_FantiaPostId,
                    _regex_FantiaUrl
                });
            }
            _re_RemoveLastDot = new Lazy<Regex>(() => new Regex(_regex_RemoveLastDot));
            _re_ProxyString = new Lazy<Regex>(() => new Regex(_regex_ProxyString));
        }

        public static Regex GetRegex(RegexType rt)
        {
            switch (rt)
            {
                case RegexType.RemoveLastDot:
                    return _re_RemoveLastDot.Value;
                case RegexType.ProxyString:
                    return _re_ProxyString.Value;
                case RegexType.PatreonCid:
                    return new Regex(_regex_PatreonCid);
                case RegexType.PatreonEmail:
                    return new Regex(_regex_PatreonEmail);
                case RegexType.PatreonHtmlImg:
                    return new Regex(_regex_PatreonHtmlImg);
                case RegexType.FanboxUrl1:
                    return new Regex(_regex_FanboxUrl1);
                case RegexType.FanboxUrl2:
                    return new Regex(_regex_FanboxUrl2);
                case RegexType.FanboxCSRF:
                    return new Regex(_regex_FanboxCSRF);
                case RegexType.FantiaIdName:
                    return new Regex(_regex_FantiaIdName);
                case RegexType.FantiaPlan:
                    return new Regex(_regex_FantiaPlan);
                case RegexType.FantiaPostId:
                    return new Regex(_regex_FantiaPostId);
                case RegexType.FantiaUrl:
                    return new Regex(_regex_FantiaUrl);
            }
            return null;
        }
    }
}
