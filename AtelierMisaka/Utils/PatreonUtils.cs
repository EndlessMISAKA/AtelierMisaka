using AtelierMisaka.Models;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public class PatreonUtils : BaseUtils
    {
        private ChromiumWebBrowser _cwb;
        private bool _needLogin = false;
        //private string _currentCookie = string.Empty;
        private readonly Regex _cidRegex = GlobalRegex.GetRegex(RegexType.PatreonCid);
        private readonly Regex _emailRegex = GlobalRegex.GetRegex(RegexType.PatreonEmail);
        private readonly Regex _htmlImg = GlobalRegex.GetRegex(RegexType.PatreonHtmlImg);
        private readonly string _postUrl = "https://www.patreon.com/api/posts?include=attachments.null%2Cmedia.null&filter[campaign_id]={0}&sort=-published_at&fields[post]=Ccomment_count%2Ccontent%2Ccurrent_user_can_view%2Ccurrent_user_has_liked%2Cembed%2Cimage%2Cpublished_at%2Cpost_type%2Cthumbnail_url%2Cteaser_text%2Ctitle%2Curl&json-api-use-default-includes=false&filter[is_draft]=false";
        private readonly string _nextUrl = "https://www.patreon.com/api/posts?include=attachments.null%2Cmedia.null&filter[campaign_id]={0}&page[cursor]={1}&sort=-published_at&fields[post]=Ccomment_count%2Ccontent%2Ccurrent_user_can_view%2Ccurrent_user_has_liked%2Cembed%2Cimage%2Cpublished_at%2Cpost_type%2Cthumbnail_url%2Cteaser_text%2Ctitle%2Curl&json-api-use-default-includes=false&filter[is_draft]=false";
        private readonly string _artistUrl = "https://www.patreon.com/api/campaigns/{0}?include=null";
        private readonly string _pledgeUrl = "https://www.patreon.com/api/pledges?include=campaign&fields[campaign]=name%2Curl&fields[pledge]=amount_cents%2Cpledge_cap_cents&json-api-use-default-includes=false";
        private readonly string _webCharSet = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/></head><body>{0}</body></html>";

        private Dictionary<string, string> _unicodeDic = new Dictionary<string, string>();

        private HashSet<string> _idList = null;

        public async Task<ResultMessage> InitBrowser()
        {
            try
            {
                if (null != GlobalData.VM_MA.PatreonCefBrowser)
                {
                    _cwb = (ChromiumWebBrowser)GlobalData.VM_MA.PatreonCefBrowser;
                    if (!GlobalData.VM_MA.IsInitialized)
                    {
                        while (!_cwb.IsBrowserInitialized)
                        {
                            await Task.Delay(500);
                        }
                        if (GlobalData.VM_MA.UseProxy)
                        {
                            if (!await CefHelper.SetProxy(_cwb, GlobalData.VM_MA.Proxy))
                            {
                                return ResultHelper.WebError(GlobalLanguage.Msg_ErrorWebProxy);
                            }
                        }
                        return await LoginCheck(await GetWebCode("view-source:https://www.patreon.com/home"));
                    }
                    return ResultHelper.NoError(false);
                }
                CefHelper.Initialize();
                _cwb = new ChromiumWebBrowser("about:blank");
                GlobalData.VM_MA.PatreonCefBrowser = _cwb;
                while (!_cwb.IsBrowserInitialized)
                {
                    await Task.Delay(500);
                }
                if (GlobalData.VM_MA.UseProxy)
                {
                    if (!await CefHelper.SetProxy(_cwb, GlobalData.VM_MA.Proxy))
                    {
                        return ResultHelper.WebError(GlobalLanguage.Msg_ErrorWebProxy);
                    }
                }
                return await LoginCheck(await GetWebCode("view-source:https://www.patreon.com/home"));
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                return ResultHelper.UnKnownError();
            }
        }

        private async Task<ResultMessage> LoginCheck(string htmlc)
        {
            if (!_needLogin)
            {
                if (!htmlc.Contains("window.patreon.userId"))
                {
                    _needLogin = true;
                    await GetWebCode("about:blank");
                    return await LoginCheck(await GetWebCode("https://www.patreon.com/login?ru=%2Fhome"));
                }
                else if (_cwb.Address.Contains("login?ru"))
                {
                    _needLogin = true;
                    await GetWebCode("about:blank");
                    return await LoginCheck(await GetWebCode("https://www.patreon.com/login?ru=%2Fhome"));
                }
                else
                {
                    if (string.IsNullOrEmpty(htmlc))
                    {
                        return ResultHelper.WebError();
                    }
                    Match ma = _emailRegex.Match(htmlc);
                    if (ma.Success)
                    {
                        var s = ma.Groups[1].Value;
                        GlobalData.VM_MA.Cookies = s;
                        GlobalData.VM_MA.IsInitialized = true;
                        return ResultHelper.NoError(_needLogin);
                    }
                    return ResultHelper.CookieError(GlobalLanguage.Msg_ErrorCookiesMail);
                }
            }
            else
            {
                GlobalData.VM_MA.ShowLogin = true;
                _cwb.FrameLoadEnd += CWebBrowser_LoginCheck;
                _cwb.FrameLoadStart += Cwb_FrameLoadStart;
                return ResultHelper.NoError(_needLogin);
            }
        }

        public async Task<ResultMessage> Logout()
        {
            await GetWebCode("https://www.patreon.com/logout?ru=%2Fhome");
            _needLogin = true;
            await GetWebCode("about:blank");
            return await LoginCheck(await GetWebCode("https://www.patreon.com/login?ru=%2Fhome"));
        }

        private void Cwb_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            if (e.Url == "https://www.patreon.com/home" || e.Url == "https://www.patreon.com/creator-home")
            {
                _cwb.Stop();
                _cwb.FrameLoadStart -= Cwb_FrameLoadStart;
                _cwb.Load("view-source:" + e.Url);
            }
        }

        public async void CWebBrowser_LoginCheck(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                string htmlc = await e.Browser.MainFrame.GetTextAsync();
                if (htmlc.Contains("window.patreon.userId"))
                {
                    Match ma = _emailRegex.Match(htmlc);
                    if (ma.Success)
                    {
                        var s = ma.Groups[1].Value;
                        //if (s != GlobalData.VM_MA.Cookies)
                        //{
                        //    GlobalData.VM_MA.Messages = GlobalLanguage.Msg_ErrorCookiesAuto;
                        //}
                        GlobalData.VM_MA.Cookies = s;
                        GlobalData.VM_MA.IsInitialized = true;
                    }
                    else
                    {
                        GlobalData.VM_MA.Messages = GlobalLanguage.Msg_ErrorCookiesMail;
                    }
                    _cwb.FrameLoadEnd -= CWebBrowser_LoginCheck;
                    _needLogin = false;
                    GlobalData.VM_MA.ShowLogin = false;
                }
            }
        }

        public async override Task<ResultMessage> GetArtistInfo(string url)
        {
            try
            {
                string ss = await GetWebCode("view-source:" + url);
                Match ma = _cidRegex.Match(ss);
                if (ma.Success)
                {
                    string _cid = ma.Groups[1].Value;
                    ss = ChangeUnicode(await GetAPI(string.Format(_artistUrl, _cid)));
                    var jpa = JsonConvert.DeserializeObject<JsonData_Patreon_Artist>(ss);
                    if (null != jpa.data)
                    {
                        var ai = new ArtistInfo()
                        {
                            Id = _cid,
                            Cid = _cid,
                            AName = GlobalMethord.RemoveLastDot(GlobalMethord.ReplacePath(jpa.data.attributes.name)),
                            PostUrl = url,
                            PayLow = GlobalData.VM_MA.Artist.PayLow,
                            PayHigh = GlobalData.VM_MA.Artist.PayHigh
                        };
                        return ResultHelper.NoError(ai);
                    }
                    return ResultHelper.IOError();
                }
                return ResultHelper.PathError();
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                return ResultHelper.UnKnownError();
            }
        }

        public async override Task<ResultMessage> GetArtistList()
        {
            try
            {
                string ss = ChangeUnicode(await GetAPI(_pledgeUrl));
                var jpp = JsonConvert.DeserializeObject<JsonData_Patreon_Pledge>(ss);
                if (null != jpp.data && null != jpp.included)
                {
                    List<ArtistInfo> ais = new List<ArtistInfo>();
                    var tais = GlobalData.VM_MA.ArtistList.ToList();
                    if (tais.Count == 0)
                    {
                        tais.Add(new ArtistInfo());
                    }
                    var incll = jpp.included.ToList();
                    for (int i = 0; i < jpp.data.Length; i++)
                    {
                        var inclu = incll.Find(x => x.id == jpp.data[i].relationships.campaign.data.id);
                        var ai = new ArtistInfo()
                        {
                            Id = inclu.id,
                            Cid = inclu.id,
                            AName = GlobalMethord.RemoveLastDot(GlobalMethord.ReplacePath(inclu.attributes.name)),
                            PostUrl = inclu.attributes.url,
                            PayHigh = jpp.data[i].attributes.amount_cents.ToString()
                        };
                        tais.Remove(ai);
                        ais.Add(ai);
                    }
                    ais.AddRange(tais);
                    return ResultHelper.NoError(ais);
                }
                if (ss == "{\"data\":[]}")
                {
                    return ResultHelper.NoError(new List<ArtistInfo>());
                }
                return ResultHelper.IOError();
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                return ResultHelper.UnKnownError();
            }
        }

        public override bool GetCover(BaseItem bi)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
                wc.Proxy = GlobalData.VM_MA.MyProxy;
                wc.DownloadDataAsync(new Uri(bi.CoverPicThumb), bi);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async override Task<ResultMessage> GetPostIDs(string uid)
        {
            try
            {
                _idList = new HashSet<string>();
                string ss = ChangeUnicode(await GetAPI(string.Format(_postUrl, uid)));
                List<BaseItem> pis = new List<BaseItem>();
                while (true)
                {
                    var jpp = JsonConvert.DeserializeObject<JsonData_Patreon_Post>(ss);
                    if (null != jpp.data && null != jpp.meta)
                    {
                        List<Included> incll = new List<Included>();
                        if (null != jpp.included)
                        {
                            incll = jpp.included.ToList();
                        }
                        for (int i = 0; i < jpp.data.Length; i++)
                        {
                            if (DateTime.TryParse(jpp.data[i].attributes.published_at, out DateTime dt))
                            {
                                if (GlobalMethord.OverTime(dt))
                                {
                                    if (dt > GlobalData.VM_MA.LastDate_End)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return ResultHelper.NoError(pis);
                                    }
                                }
                            }

                            PatreonItem pi = new PatreonItem()
                            {
                                CreateDate = dt,
                                UpdateDate = dt,
                                PID = jpp.data[i].id,
                                Title = GlobalMethord.RemoveAllDot(GlobalMethord.ReplacePath(jpp.data[i].attributes.title)),
                                IsLiked = jpp.data[i].attributes.current_user_has_liked,
                                PLink = jpp.data[i].attributes.url
                            };
                            if (!_idList.Add(pi.ID))
                            {
                                return ResultHelper.NoError(pis);
                            }

                            GlobalData.VM_MA.PostTitle = pi.Title;
                            if (!string.IsNullOrEmpty(jpp.data[i].attributes.content))
                            {
                                pi.Comments.Add(_htmlImg.Replace(jpp.data[i].attributes.content, ""));
                            }
                            else if (!string.IsNullOrEmpty(jpp.data[i].attributes.content_teaser_text))
                            {
                                pi.Comments.Add(jpp.data[i].attributes.content_teaser_text);
                            }
                            if (jpp.data[i].attributes.current_user_can_view)
                            {
                                if (null != jpp.data[i].attributes.image)
                                {
                                    pi.CoverPicThumb = jpp.data[i].attributes.image.thumb_url;
                                }
                                if (null != jpp.data[i].attributes.embed)
                                {
                                    pi.Comments.Add($"<{GlobalLanguage.Text_LinkPref} {jpp.data[i].attributes.embed.url} >");
                                }

                                if (null != jpp.data[i].relationships.media)
                                {
                                    for (int j = 0; j < jpp.data[i].relationships.media.data.Length; j++)
                                    {
                                        var inclu = incll.Find(x => x.id == jpp.data[i].relationships.media.data[j].id);
                                        if (string.IsNullOrEmpty(inclu.attributes.file_name))
                                        {
                                            inclu.attributes.file_name = "default.";
                                            if (!string.IsNullOrEmpty(inclu.attributes.mimetype))
                                            {
                                                var tep = inclu.attributes.mimetype.Split('/');
                                                if (tep.Length == 2)
                                                {
                                                    inclu.attributes.file_name += tep[1];
                                                }
                                                else
                                                {
                                                    inclu.attributes.file_name += inclu.attributes.mimetype;
                                                }
                                            }
                                            else
                                                inclu.attributes.file_name += "png";
                                        }
                                        else if (inclu.attributes.file_name.StartsWith("https://"))
                                        {
                                            continue;
                                        }
                                        else if (inclu.attributes.file_name.Equals("jpg") || inclu.attributes.file_name.Equals("png"))
                                        {
                                            inclu.attributes.file_name = $"{inclu.id}.{inclu.attributes.file_name}";
                                        }
                                        pi.ContentUrls.Add(inclu.attributes.download_url);
                                        pi.FileNames.Add(inclu.attributes.file_name);
                                        pi.Comments.Add($"<{GlobalLanguage.Text_FilePref} {inclu.attributes.file_name}>");
                                    }
                                }
                            }
                            pis.Add(pi);
                            GlobalData.VM_MA.PostCount++;
                        }
                        if (null != jpp.meta.pagination.cursors)
                        {
                            ss = ChangeUnicode(await GetAPI(string.Format(_nextUrl, uid, jpp.meta.pagination.cursors.next)));
                            continue;
                        }
                        return ResultHelper.NoError(pis);
                    }
                    GlobalMethord.ErrorLog("Json Error" + Environment.NewLine + ss + Environment.NewLine + "-----------------------------------------------");
                    return ResultHelper.IOError();
                }
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                return ResultHelper.UnKnownError();
            }
        }

        public async override Task<ResultMessage> LikePost(string pid, string cid)
        {
            return await Task.Run(() => ResultHelper.UnKnownError(GlobalLanguage.Msg_ErrorUnSupported));
        }

        private async Task<string> GetWebCode(string url)
        {
            _cwb.Load(url);
            do
            {
                await Task.Delay(100);
            } while (_cwb.IsLoading);
            return await _cwb.GetTextAsync();
        }

        private async Task<string> GetAPI(string url)
        {
            _cwb.Load(url);
            do
            {
                await Task.Delay(100);
            } while (_cwb.IsLoading);
            return await _cwb.GetTextAsync();
        }

        private async Task<string> GetWebContent(string content)
        {
            _cwb.LoadHtml(string.Format(_webCharSet, content));
            do
            {
                await Task.Delay(150);
            } while (_cwb.IsLoading);
            return await _cwb.GetTextAsync();
        }
        
        private string ChangeUnicode(string str)
        {
            return str.Replace("\\u00a0", " ").Replace("\\u00A0", " ");
        }
    }
}
