using AtelierMisaka.Models;
using AtelierMisaka.Utils;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public class PatreonUtils : BaseUtils
    {
        private ChromiumWebBrowser _cwb;
        private readonly Regex _cidRegex = GlobalRegex.GetRegex(RegexType.PatreonCid);
        private readonly Regex _emailRegex = GlobalRegex.GetRegex(RegexType.PatreonEmail);
        private readonly Regex _htmlImg = GlobalRegex.GetRegex(RegexType.PatreonHtmlImg);
        private readonly string _postUrl = "https://www.patreon.com/api/posts?include=attachments.null%2Cmedia.null&filter[campaign_id]={0}&sort=-published_at&fields[post]=Ccomment_count%2Ccontent%2Ccurrent_user_can_view%2Ccurrent_user_has_liked%2Cembed%2Cimage%2Cpublished_at%2Cpost_type%2Cthumbnail_url%2Cteaser_text%2Ctitle%2Curl&json-api-use-default-includes=false&filter[is_draft]=false";
        private readonly string _nextUrl = "https://www.patreon.com/api/posts?include=attachments.null%2Cmedia.null&filter[campaign_id]={0}&page[cursor]={1}&sort=-published_at&fields[post]=Ccomment_count%2Ccontent%2Ccurrent_user_can_view%2Ccurrent_user_has_liked%2Cembed%2Cimage%2Cpublished_at%2Cpost_type%2Cthumbnail_url%2Cteaser_text%2Ctitle%2Curl&json-api-use-default-includes=false&filter[is_draft]=false";
        private readonly string _artistUrl = "https://www.patreon.com/api/campaigns/{0}?include=null";
        private readonly string _pledgeUrl = "https://www.patreon.com/api/pledges?include=campaign&fields[campaign]=name%2Curl&fields[pledge]=amount_cents%2Cpledge_cap_cents&json-api-use-default-includes=false";
        private readonly string _webCharSet = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/></head><body>{0}</body></html>";

        readonly string _meUrl = "https://www.patreon.com/api/current_user?include=null&fields[user]=full_name%2Curl&json-api-version=1.0";
        private Dictionary<string, string> _unicodeDic = new Dictionary<string, string>();

        private HashSet<string> _idList = null;

        public async override Task<ResultMessage> CheckCookies()
        {
            try
            {
                var data = await GetAPI(_meUrl);
                //{"errors":[{"code":53,"code_name":"LoginRequired","detail":"This route is restricted to logged in users.","status":"401","title":"This route is restricted to logged in users."}]}
                var json = JsonHelper.GetJObject(data);
                if (json.ContainsKey("errors"))
                {
                    _cwb.Load("https://www.patreon.com");
                    return ResultHelper.NoError(null);
                }
                var ff = json.SelectToken("data.attributes.full_name");
                if (ff == null)
                {
                    throw new Exception("API(patreon.com/api/current_user) has changed");
                }
                if (ff.Value<string>() != GlobalData.VM_MA.IDName)
                {
                    _cwb.Load("https://www.patreon.com");
                    return ResultHelper.NoError(false);
                }
                return ResultHelper.NoError(true);
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                return ResultHelper.UnKnownError();
            }
        }

        public async override Task<ResultMessage> GetArtistInfo(string url)
        {
            try
            {
                string ss = await GetAPI("view-source:" + url);
                Match ma = _cidRegex.Match(ss);
                if (ma.Success)
                {
                    string _cid = ma.Groups[1].Value;
                    ss = await GetAPI(string.Format(_artistUrl, _cid));
                    var jpa = JsonHelper.ToObject<JsonData_Patreon_Artist>(ss);
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
                string ss = await GetAPI(_pledgeUrl);
                var jpp = JsonHelper.ToObject<JsonData_Patreon_Pledge>(ss);
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
                string ss = await GetAPI(string.Format(_postUrl, uid));
                List<BaseItem> pis = new List<BaseItem>();
                while (true)
                {
                    var jpp = JsonHelper.ToObject<JsonData_Patreon_Post>(ss);
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
                            ss = await GetAPI(string.Format(_nextUrl, uid, jpp.meta.pagination.cursors.next));
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

        private async Task<string> GetAPI(string url)
        {
            if (_cwb == null)
            {
                _cwb = (ChromiumWebBrowser)GlobalData.VM_MA.PatreonCefBrowser;
            }
            _cwb.Load(url);
            do
            {
                await Task.Delay(100);
            } while (_cwb.IsLoading);
            return ChangeUnicode(await _cwb.GetTextAsync());
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
