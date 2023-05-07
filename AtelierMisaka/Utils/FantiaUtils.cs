using AtelierMisaka.Models;
using AtelierMisaka.Utils;
using CefSharp;
using CefSharp.Handler;
using CefSharp.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public class FantiaUtils : BaseUtils
    {
        private ChromiumWebBrowser _cwb;
        readonly Regex _artIdName = GlobalRegex.GetRegex(RegexType.FantiaIdName);
        readonly Regex _artPlan = GlobalRegex.GetRegex(RegexType.FantiaPlan);
        readonly Regex _artPost = GlobalRegex.GetRegex(RegexType.FantiaPostId);
        readonly Regex _artUrl = GlobalRegex.GetRegex(RegexType.FantiaUrl);
        readonly Regex _artDataImage = GlobalRegex.GetRegex(RegexType.FantiaDataImage);
        readonly string _nextP = "fa fa-angle-right";
        readonly string _meUrl = "https://fantia.jp/api/v1/me";
        private HashSet<string> _pidList = null;
        public Queue<string> _apiData = new Queue<string>();


        public async override Task<ResultMessage> CheckCookies()
        {
            try
            {
                var data = await GetWebCodeAsync(_meUrl);
                //{"redirect_to":"/sessions/signin"}
                var json = JsonHelper.GetJObject(data);
                if (json.ContainsKey("redirect_to"))
                {
                    _cwb.Load("https://fantia.jp");
                    return ResultHelper.NoError(null);
                }
                var ff = json.SelectToken("current_user.name");
                if (ff == null)
                {
                    throw new Exception("API(fantia.jp/api/v1/me) has changed");
                }
                if (ff.Value<string>() != GlobalData.VM_MA.IDName)
                {
                    _cwb.Load("https://fantia.jp");
                    return ResultHelper.NoError(false);
                }
                var cookies = await _cwb.GetCookieManager().VisitUrlCookiesAsync("https://fantia.jp", true);
                GlobalData.VM_MA.Cookies = cookies.Find(x => x.Name == "_session_id").Value;
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
                Match ma = _artUrl.Match(url);
                if (!ma.Success)
                {
                    return ResultHelper.PathError();
                }
                string cid = ma.Groups[1].Value;

                var jfa = JsonHelper.ToObject<JsonData_Fantia_Artist>(await GetWebCodeAsync($"view-source:https://fantia.jp/api/v1/fanclubs/{cid}"));
                if (null != jfa.fanclub)
                {
                    var ai = new ArtistInfo()
                    {
                        Id = cid,
                        AName = GlobalMethord.RemoveLastDot(GlobalMethord.ReplacePath(jfa.fanclub.creator_name)),
                        Cid = cid,
                        PostUrl = $"https://fantia.jp/fanclubs/{cid}",
                        PayLow = GlobalData.VM_MA.Artist.PayLow,
                        PayHigh = GlobalData.VM_MA.Artist.PayHigh
                    };
                    return ResultHelper.NoError(ai);
                }
                return ResultHelper.IOError();
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
                List<ArtistInfo> ais = new List<ArtistInfo>();
                //有料
                ais.AddRange(await GetArtistListFromWebCode("not_"));
                //無料
                ais.AddRange(await GetArtistListFromWebCode(string.Empty));

                var tais = GlobalData.VM_MA.ArtistList.ToList();
                if (tais.Count == 0)
                {
                    tais.Add(new ArtistInfo());
                }
                tais.ForEach(x =>
                {
                    if (!ais.Contains(x))
                        ais.Add(x);
                });
                return ResultHelper.NoError(ais);
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                return ResultHelper.UnKnownError();
            }
        }

        private async Task<List<ArtistInfo>> GetArtistListFromWebCode(string free, int index = 1)
        {
            List<ArtistInfo> ais = new List<ArtistInfo>();
            try
            {
                string sphtml = await GetWebCodeAsync($"view-source:https://fantia.jp/mypage/users/plans?page={index}&type={free}free");
                Match ma = _artIdName.Match(sphtml);
                while (ma.Success)
                {
                    var cid = ma.Groups[1].Value;
                    var ana = ma.Groups[2].Value;
                    int ind = ana.IndexOf('(');
                    if (ind != -1)
                        ana = ana.Substring(0, ind);
                    var ai = new ArtistInfo()
                    {
                        Id = cid,
                        Cid = cid,
                        AName = GlobalMethord.RemoveLastDot(GlobalMethord.ReplacePath(ana)),
                        PostUrl = $"https://fantia.jp/fanclubs/{cid}",
                        PayHigh = "0"
                    };
                    ais.Add(ai);
                    ma = ma.NextMatch();
                }
                if (!string.IsNullOrEmpty(free))
                {
                    ma = _artPlan.Match(sphtml);
                    int i = 0;
                    while (ma.Success)
                    {
                        ais[i++].PayHigh = ma.Groups[1].Value.Replace(",", "");
                        ma = ma.NextMatch();
                    }
                }
                if (sphtml.IndexOf(_nextP) != -1)
                {
                    index++;
                    ais.AddRange(await GetArtistListFromWebCode(free, index));
                }
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
            }
            return ais;
        }

        public override bool GetCover(BaseItem bi)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
                wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                if (GlobalData.VM_MA.UseProxy)
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
                _pidList = new HashSet<string>();
                var bis = await GetPostIDsFromWebCode(uid);
                return ResultHelper.NoError(bis);
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                if (ex is InvalidDataException)
                {
                    return ResultHelper.UnKnownError("Post Error: " + ex.Message);
                }
                return ResultHelper.UnKnownError();
            }
        }

        private async Task<IList<BaseItem>> GetPostIDsFromWebCode(string uid, int index = 1)
        {
            try
            {
                List<BaseItem> bis = new List<BaseItem>();
                string sphtml = await GetWebCodeAsync($"view-source:https://fantia.jp/fanclubs/{uid}/posts?page={index}&utf8=%E2%9C%93&q%5Bs%5D=newer");
                Match ma = _artPost.Match(sphtml);
                bool flag = true;
                while (ma.Success)
                {
                    string pid = ma.Groups[1].Value;
                    if (_pidList.Add(pid))
                    {
                        var res = await GetUrls(pid, bis);
                        if (res == false)
                        {
                            flag = false;
                            break;
                        }
                        else if (res == null)
                        {
                            throw new InvalidDataException(pid);
                        }
                    }
                    ma = ma.NextMatch();
                }
                if (flag && sphtml.IndexOf(_nextP) != -1)
                {
                    index++;
                    bis.AddRange(await GetPostIDsFromWebCode(uid, index));
                }
                return bis;
            }
            catch
            {
                throw;
            }
        }

        private async Task<bool?> GetUrls(string pid, List<BaseItem> bis)
        {
            try
            {
                var jfp = JsonHelper.ToObject<JsonData_Fantia_Post>(await GetApiCodeAsync($"https://fantia.jp/posts/{pid}"));
                //var jfp = JsonHelper.ToObject<JsonData_Fantia_Post>(pid); //for test
                if (null != jfp.post)
                {
                    FantiaItem fi = new FantiaItem();
                    if (DateTime.TryParse(jfp.post.posted_at, out DateTime dt))
                    {
                        fi.CreateDate = dt;
                    }
                    if (DateTime.TryParse(jfp.post.converted_at, out dt))
                    {
                        fi.UpdateDate = dt;
                    }

                    if (GlobalMethord.OverTime(fi.UpdateDate))
                    {
                        if (GlobalMethord.OverTime(fi.CreateDate))
                        {
                            return fi.UpdateDate > GlobalData.VM_MA.LastDate;
                        }
                    }
                    fi.FID = jfp.post.id.ToString();
                    fi.Title = GlobalMethord.RemoveAllDot(GlobalMethord.ReplacePath(jfp.post.title));
                    GlobalData.VM_MA.PostTitle = fi.Title;
                    if (!string.IsNullOrEmpty(jfp.post.comment))
                    {
                        fi.Comments.Add(jfp.post.comment);
                        fi.Comments.Add(string.Empty);
                    }
                    if (null != jfp.post.thumb)
                    {
                        fi.CoverPic = jfp.post.thumb.original;
                        fi.CoverPicThumb = jfp.post.thumb.ogp;
                    }
                    if (DateTime.TryParse(jfp.post.deadline, out dt))
                    {
                        fi.DeadDate = dt.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    else
                    {
                        fi.DeadDate = "---";
                    }

                    foreach (var ct in jfp.post.post_contents)
                    {
                        var fee = 0;
                        if (null != ct.plan)
                        {
                            fee = ct.plan.price;
                        }
                        var stitle = $"${fee}_{GlobalMethord.RemoveAllDot(GlobalMethord.ReplacePath(ct.title))}";
                        fi.Comments.Add("------------------------------------------------------------------------------------------");
                        fi.Comments.Add(stitle);
                        fi.Comments.Add(string.Empty);
                        if (ct.visible_status == "visible")
                        {
                            if (ct.category != "blog" && !string.IsNullOrEmpty(ct.comment))
                            {
                                fi.Comments.Add(ct.comment);
                                fi.Comments.Add(string.Empty);
                            }
                            if (ct.category == "photo_gallery")
                            {
                                var imgs = ct.post_content_photos;
                                foreach (var img in imgs)
                                {
                                    var imgUrl = img.url.original;
                                    if (!string.IsNullOrEmpty(img.comment))
                                    {
                                        fi.Comments.Add(img.comment);
                                    }
                                    var ffn = imgUrl.Substring(0, imgUrl.IndexOf("?Key"));
                                    var ext = ffn.Substring(ffn.LastIndexOf('.'));
                                    var fn = $"{img.id}{ext}";
                                    fi.Comments.Add($"<{GlobalLanguage.Text_ImagePref} {fn}>");
                                    fi.FileNames.Add(fn);
                                    fi.ContentUrls.Add(imgUrl);
                                    fi.Fees.Add($"{fee}");
                                    fi.PTitles.Add(stitle);
                                }
                            }
                            else if (ct.category == "file")
                            {
                                fi.Comments.Add($"<{GlobalLanguage.Text_FilePref} {ct.filename}>");
                                fi.FileNames.Add(ct.filename);
                                fi.ContentUrls.Add($"https://fantia.jp{ct.download_uri}");
                                fi.Fees.Add($"{fee}");
                                fi.PTitles.Add(stitle);
                            }
                            else if (ct.category == "blog")
                            {
                                try
                                {
                                    JObject dd = JsonHelper.GetJObject(ct.comment);
                                    JArray ja = JArray.Parse(dd["ops"].ToString());

                                    foreach (var js in ja)
                                    {
                                        var ss = js.SelectToken("insert");
                                        dynamic stem = ss;
                                        if (ss.Type == JTokenType.String)
                                        {
                                            fi.Comments.Add(stem.Value.Replace("\\n", Environment.NewLine));
                                        }
                                        else if (ss.Type == JTokenType.Object)
                                        {
                                            if (null == stem.fantiaImage)
                                            {
                                                fi.Comments.Add(stem.ToString());
                                            }
                                            else
                                            {
                                                string imgUrl = stem.fantiaImage.url;
                                                string fn = string.Empty;
                                                if (imgUrl.StartsWith("http"))
                                                {
                                                    var ffn = imgUrl.Substring(0, imgUrl.IndexOf("?Key"));
                                                    var ext = ffn.Substring(ffn.LastIndexOf('.'));
                                                    fn = $"{stem.fantiaImage.id}{ext}";
                                                    fi.Comments.Add($"<{GlobalLanguage.Text_ImagePref} {fn}>");
                                                }
                                                else
                                                {
                                                    Match ma = _artDataImage.Match(imgUrl);
                                                    if (ma.Success)
                                                    {
                                                        fn = $"dimg:{stem.fantiaImage.id}.{ma.Groups[1].Value}";
                                                        fi.Comments.Add($"<{GlobalLanguage.Text_ImagePref} {fn}>");
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("Blog Image Type Error: " + imgUrl);
                                                    }
                                                }
                                                fi.FileNames.Add(fn);
                                                fi.ContentUrls.Add($"https://fantia.jp{stem.fantiaImage.original_url}");
                                                fi.Fees.Add($"{fee}");
                                                fi.PTitles.Add(stitle);
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Blog type unknown: " + ss.Type.ToString());
                                        }
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    bis.Add(fi);
                    GlobalData.VM_MA.PostCount++;
                    GlobalData.VM_DL.AddFantiaCommand.Execute(fi);
                    do
                    {
                        await Task.Delay(2000);
                    } while (GlobalData.VM_DL.WaitDownloading);

                    return true;
                }
                return null;
            }
            catch
            {
                throw;
            }
        }

        public async Task<FantiaItem> GetUrls(string pid)
        {
            try
            {
                var jfp = JsonHelper.ToObject<JsonData_Fantia_Post>(await GetApiCodeAsync($"https://fantia.jp/posts/{pid}"));
                if (null != jfp.post)
                {
                    FantiaItem fi = new FantiaItem();
                    if (DateTime.TryParse(jfp.post.posted_at, out DateTime dt))
                    {
                        fi.CreateDate = dt;
                    }
                    if (DateTime.TryParse(jfp.post.converted_at, out dt))
                    {
                        fi.UpdateDate = dt;
                    }

                    fi.FID = jfp.post.id.ToString();
                    fi.Title = GlobalMethord.RemoveAllDot(GlobalMethord.ReplacePath(jfp.post.title));
                    GlobalData.VM_MA.PostTitle = fi.Title;
                    if (!string.IsNullOrEmpty(jfp.post.comment))
                    {
                        fi.Comments.Add(jfp.post.comment);
                        fi.Comments.Add(string.Empty);
                    }
                    if (null != jfp.post.thumb)
                    {
                        fi.CoverPic = jfp.post.thumb.original;
                        fi.CoverPicThumb = jfp.post.thumb.ogp;
                    }
                    if (DateTime.TryParse(jfp.post.deadline, out dt))
                    {
                        fi.DeadDate = dt.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    else
                    {
                        fi.DeadDate = "---";
                    }

                    foreach (var ct in jfp.post.post_contents)
                    {
                        var fee = 0;
                        if (null != ct.plan)
                        {
                            fee = ct.plan.price;
                        }
                        var stitle = $"${fee}_{GlobalMethord.RemoveAllDot(GlobalMethord.ReplacePath(ct.title))}";
                        fi.Comments.Add("------------------------------------------------------------------------------------------");
                        fi.Comments.Add(stitle);
                        fi.Comments.Add(string.Empty);
                        if (ct.visible_status == "visible")
                        {
                            if (ct.category != "blog" && !string.IsNullOrEmpty(ct.comment))
                            {
                                fi.Comments.Add(ct.comment);
                                fi.Comments.Add(string.Empty);
                            }
                            if (ct.category == "photo_gallery")
                            {
                                var imgs = ct.post_content_photos;
                                foreach (var img in imgs)
                                {
                                    var imgUrl = img.url.original;
                                    if (!string.IsNullOrEmpty(img.comment))
                                    {
                                        fi.Comments.Add(img.comment);
                                    }
                                    var ffn = imgUrl.Substring(0, imgUrl.IndexOf("?Key"));
                                    var ext = ffn.Substring(ffn.LastIndexOf('.'));
                                    var fn = $"{img.id}{ext}";
                                    fi.Comments.Add($"<{GlobalLanguage.Text_ImagePref} {fn}>");
                                    fi.FileNames.Add(fn);
                                    fi.ContentUrls.Add(imgUrl);
                                    fi.Fees.Add($"{fee}");
                                    fi.PTitles.Add(stitle);
                                }
                            }
                            else if (ct.category == "file")
                            {
                                fi.Comments.Add($"<{GlobalLanguage.Text_FilePref} {ct.filename}>");
                                fi.FileNames.Add(ct.filename);
                                fi.ContentUrls.Add($"https://fantia.jp{ct.download_uri}");
                                fi.Fees.Add($"{fee}");
                                fi.PTitles.Add(stitle);
                            }
                            else if (ct.category == "blog")
                            {
                                try
                                {
                                    JObject dd = JsonHelper.GetJObject(ct.comment);
                                    JArray ja = JArray.Parse(dd["ops"].ToString());

                                    foreach (var js in ja)
                                    {
                                        var ss = js.SelectToken("insert");
                                        dynamic stem = ss;
                                        if (ss.Type == JTokenType.String)
                                        {
                                            fi.Comments.Add(stem.Value.Replace("\\n", Environment.NewLine));
                                        }
                                        else if (ss.Type == JTokenType.Object)
                                        {
                                            string imgUrl = stem.fantiaImage.url;
                                            string fn = string.Empty;
                                            if (imgUrl.StartsWith("http"))
                                            {
                                                var ffn = imgUrl.Substring(0, imgUrl.IndexOf("?Key"));
                                                var ext = ffn.Substring(ffn.LastIndexOf('.'));
                                                fn = $"{stem.fantiaImage.id}{ext}";
                                                fi.Comments.Add($"<{GlobalLanguage.Text_ImagePref} {fn}>");
                                            }
                                            else
                                            {
                                                Match ma = _artDataImage.Match(imgUrl);
                                                if (ma.Success)
                                                {
                                                    fn = $"dimg:{stem.fantiaImage.id}.{ma.Groups[1].Value}";
                                                    fi.Comments.Add($"<{GlobalLanguage.Text_ImagePref} {fn}>");

                                                }
                                                else
                                                {
                                                    throw new Exception("Blog Image Type Error: " + imgUrl);
                                                }
                                            }
                                            fi.FileNames.Add(fn);
                                            fi.ContentUrls.Add($"https://fantia.jp{stem.fantiaImage.original_url}");
                                            fi.Fees.Add($"{fee}");
                                            fi.PTitles.Add(stitle);
                                        }
                                        else
                                        {
                                            throw new Exception("Blog type unknown: " + ss.Type.ToString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                                }
                            }
                        }
                    }
                    return fi;
                }
                return null;
            }
            catch (Exception ex)
            {
                GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                return null;
            }
        }



        private async Task<string> GetApiCodeAsync(string url)
        {
            if (_cwb == null)
            {
                _cwb = (ChromiumWebBrowser)GlobalData.VM_MA.PatreonCefBrowser;
            }
            if (_cwb.RequestHandler == null)
            {
                _cwb.RequestHandler = new FantiaRequestHandle(_apiData);
            }
            _apiData.Clear();
            _cwb.Load(url);
            int count = 0;
            while (true)
            {
                await Task.Delay(200);
                if (_apiData.Count > 0)
                {
                    return _apiData.Dequeue();
                }
                if (++count == 30)
                {
                    return "";
                }
                if (!_cwb.IsLoading)
                {
                    if (_apiData.Count > 0)
                    {
                        return _apiData.Dequeue();
                    }
                    await Task.Delay(500);
                    break;
                }
            }
            if (_apiData.Count > 0)
            {
                return _apiData.Dequeue();
            }
            else
            {
                return "";
            }
        }

        private async Task<string> GetWebCodeAsync(string url)
        {
            if (_cwb == null)
            {
                _cwb = (ChromiumWebBrowser)GlobalData.VM_MA.PatreonCefBrowser;
            }
            if (_cwb.RequestHandler == null)
            {
                _cwb.RequestHandler = new FantiaRequestHandle(_apiData);
            }
            _cwb.Load(url);
            do
            {
                await Task.Delay(100);
            } while (_cwb.IsLoading);
            return await _cwb.GetTextAsync();
        }

        //private async Task<string> GetWebCodeAsync(string url)
        //{
        //    try
        //    {
        //        var wc = new TimeOutWebClient();
        //        wc.Headers.Add(HttpRequestHeader.AcceptCharset, "utf-8");
        //        wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
        //        if (GlobalData.VM_MA.UseProxy)
        //        {
        //            wc.Proxy = GlobalData.VM_MA.MyProxy;
        //        }
        //        var ss = await wc.DownloadStringTaskAsync(url);
        //        return ss;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        //private string GetWebCode(string url)
        //{
        //    WebClient wc = new WebClient();
        //    try
        //    {
        //        wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
        //        if (GlobalData.VM_MA.UseProxy)
        //        {
        //            wc.Proxy = GlobalData.VM_MA.MyProxy;
        //        }
        //        var ss = wc.DownloadData(url);
        //        string s = Encoding.UTF8.GetString(ss);
        //        return s;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        wc.Dispose();
        //    }
        //}

        public async override Task<ResultMessage> LikePost(string pid, string cid)
        {
            return await Task.Run(() => ResultHelper.UnKnownError(GlobalLanguage.Msg_ErrorUnSupported));
        }
    }


    public class FantiaRequestHandle : RequestHandler
    {
        APIResourceRequestHandler aph;

        public FantiaRequestHandle(object queue)
        {
            aph = new APIResourceRequestHandler(queue);
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            var url = request.Url.ToLower();
            if (url.StartsWith("https://fantia.jp/api/v1/post") && !url.EndsWith("specials"))
            {
                return aph;
            }
            if (GlobalData.VM_MA.NeedCookie && url == "https://fantia.jp/api/v1/me")
            {
                return aph;
            }
            return base.GetResourceRequestHandler(chromiumWebBrowser, browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
        }
    }

    public class APIResourceRequestHandler : ResourceRequestHandler
    {
        private MemoryStream ms = new MemoryStream();
        Queue<string> _queq;

        public APIResourceRequestHandler(object queq)
        {
            _queq = (Queue<string>)queq;
        }

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return new CefSharp.ResponseFilter.StreamResponseFilter(ms);
        }

        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            var str = Encoding.UTF8.GetString(ms.ToArray());
            ms = new MemoryStream();
            _queq.Enqueue(str);
        }
    }
}
