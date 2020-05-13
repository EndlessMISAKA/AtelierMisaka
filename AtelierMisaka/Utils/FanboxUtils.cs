using AtelierMisaka.Models;
using Newtonsoft.Json;
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
    public class FanboxUtils : BaseUtils
    {
        string _referer = string.Empty;
        string _x_csrf_token = string.Empty;
        Regex _artUrl = new Regex(@"^https://www\.fanbox\.cc/@(\w+)$");
        Regex _artUrl2 = new Regex(@"^https://(\w+)\.fanbox\.cc/?$");
        Regex _csrfToken = new Regex("csrfToken\":\"(\\w+)\"");

        public async override Task<ResultMessage> GetArtistInfo(string url)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Match ma = _artUrl.Match(url);
                    if (!ma.Success)
                    {
                        ma = _artUrl2.Match(url);
                        if (!ma.Success)
                        {
                            return ResultHelper.PathError();
                        }
                    }
                    _referer = $"https://www.fanbox.cc/@{ma.Groups[1].Value}";

                    var ai = GetInfo(ma.Groups[1].Value);
                    if (null != ai)
                    {
                        ai.PayLow = GlobalData.VM_MA.Artist.PayLow;
                        ai.PayHigh = GlobalData.VM_MA.Artist.PayHigh;
                        return ResultHelper.NoError(ai);
                    }
                    return ResultHelper.IOError();
                }
                catch (Exception ex)
                {
                    GlobalData.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                    if (ex is WebException || ex is System.Net.Sockets.SocketException)
                    {
                        return ex.Message.Contains("40") ? ResultHelper.CookieError() : ResultHelper.WebError();
                    }
                    return ResultHelper.UnKnownError();
                }
            });
        }

        public async override Task<ResultMessage> GetArtistList()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var ais = new List<ArtistInfo>();
                    var tais = GlobalData.VM_MA.ArtistList.ToList();
                    if (tais.Count == 0)
                    {
                        tais.Add(new ArtistInfo());
                    }

                    var jfp = JsonConvert.DeserializeObject<JsonData_Fanbox_Plan>(GetWebCode("https://api.fanbox.cc/plan.listSupporting", "https://www.fanbox.cc/creators/supporting"));
                    if (null != jfp.body)
                    {
                        foreach (var pl in jfp.body)
                        {
                            var ai = GetInfo(pl.creatorId, $"https://www.fanbox.cc/@{pl.creatorId}");
                            if (null != ai)
                            {
                                ai.PayHigh = pl.fee.ToString();
                                tais.Remove(ai);
                                ais.Add(ai);
                            }
                        }
                        ais.AddRange(tais);
                        return ResultHelper.NoError(ais);
                    }
                    return ResultHelper.IOError();
                }
                catch (Exception ex)
                {
                    GlobalData.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                    if (ex is WebException || ex is System.Net.Sockets.SocketException)
                    {
                        return ex.Message.Contains("40") ? ResultHelper.CookieError() : ResultHelper.WebError();
                    }
                    return ResultHelper.UnKnownError();
                }
            });
        }

        private ArtistInfo GetInfo(string cid, string referer = null)
        {
            try
            {
                var jfa = JsonConvert.DeserializeObject<JsonData_Fanbox_Artist>(GetWebCode($"https://api.fanbox.cc/creator.get?creatorId={cid}", referer));
                if (null != jfa.body)
                {
                    var ai = new ArtistInfo()
                    {
                        Id = jfa.body.user.userId,
                        Cid = cid,
                        AName = GlobalData.RemoveLastDot(GlobalData.ReplacePath(jfa.body.user.name.Trim())),
                        PostUrl = referer ?? _referer
                    };
                    foreach (var link in jfa.body.profileLinks)
                    {
                        if (link.Contains("twitter.com"))
                        {
                            ai.Twitter = $"{link}/status";
                            break;
                        }
                    }
                    return ai;
                }
                return null;
            }
            catch
            {
                throw;
            }
        }

        public override bool GetCover(BaseItem bi)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
                wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                wc.Headers.Add(HttpRequestHeader.Referer, GlobalData.VM_MA.Artist.PostUrl);
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
            return await Task.Run(() =>
            {
                var bis = new List<BaseItem>();
                try
                {
                    string url = $"https://api.fanbox.cc/post.listCreator?creatorId={uid}&limit=10";
                    if (string.IsNullOrEmpty(_referer))
                    {
                        _referer = $"https://{uid}.fanbox.cc";
                    }

                    string nurl = GetUrls(GetWebCode(url), bis);
                    if (!string.IsNullOrEmpty(nurl))
                    {
                        GetPostIDs_Next(nurl, bis);
                    }
                    return ResultHelper.NoError(bis);
                }
                catch (Exception ex)
                {
                    GlobalData.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                    if (ex is WebException || ex is System.Net.Sockets.SocketException)
                    {
                        return ex.Message.Contains("401") ? ResultHelper.CookieError() : ResultHelper.WebError();
                    }
                    return ResultHelper.UnKnownError();
                }
            });
        }

        private void GetPostIDs_Next(string url, IList<BaseItem> bis)
        {
            try
            {
                string nurl = GetUrls(GetWebCode(url), bis);
                if (!string.IsNullOrEmpty(nurl))
                {
                    GetPostIDs_Next(nurl, bis);
                }
            }
            catch
            {
                throw;
            }
        }

        private string GetUrls(string jsondata, IList<BaseItem> bis)
        {
            var fd = JsonConvert.DeserializeObject<JsonData_Fanbox_Post>(jsondata);
            if (null != fd.body && null != fd.body.items)
            {
                foreach (var po in fd.body.items)
                {
                    var pi = new FanboxItem()
                    {
                        PID = po.id,
                        Fee = po.feeRequired.ToString(),
                        Title = GlobalData.RemoveLastDot(GlobalData.ReplacePath(po.title.Trim())),
                        CoverPic = po.coverImageUrl,
                        CoverPicThumb = po.coverImageUrl,
                        IsLiked = po.isLiked
                    };
                    if (DateTime.TryParse(po.publishedDatetime, out DateTime dt))
                    {
                        pi.CreateDate = dt;
                    }
                    if (DateTime.TryParse(po.updatedDatetime, out dt))
                    {
                        pi.UpdateDate = dt;
                    }
                    if (GlobalData.OverPayment(int.Parse(pi.Fee)) || GlobalData.OverTime(pi.UpdateDate))
                    {
                        pi.Skip = true;
                    }

                    if (null != po.body)
                    {
                        switch (po.type)
                        {
                            case "file":
                                {
                                    if (!string.IsNullOrEmpty(po.body.text))
                                    {
                                        pi.Comments.Add(po.body.text);
                                        pi.Comments.Add(string.Empty);
                                    }
                                    foreach (var finfo in po.body.files)
                                    {
                                        pi.ContentUrls.Add(finfo.url);
                                        var fn = $"{finfo.name}.{finfo.extension}";
                                        pi.FileNames.Add(fn);
                                        pi.Comments.Add($"<文件: {fn} ({GetSize(finfo.size)})>");
                                    }
                                }
                                break;
                            case "image":
                                {
                                    if (!string.IsNullOrEmpty(po.body.text))
                                    {
                                        pi.Comments.Add(po.body.text);
                                        pi.Comments.Add(string.Empty);
                                    }
                                    int index = 1;
                                    foreach (var iinfo in po.body.images)
                                    {
                                        pi.MediaUrls.Add(iinfo.originalUrl);
                                        var fn = $"{index++}.{iinfo.extension}";
                                        pi.MediaNames.Add(fn);
                                        pi.Comments.Add($"<图片: {fn} ({iinfo.width}x{iinfo.height}px)>");
                                    }
                                }
                                break;
                            case "article":
                                {
                                    int index_pic = 1;
                                    foreach (var binfo in po.body.blocks)
                                    {
                                        switch (binfo.type)
                                        {
                                            case "p":
                                                pi.Comments.Add(binfo.text);
                                                break;
                                            case "file":
                                                if (null != po.body.fileMap && po.body.fileMap.TryGetValue(binfo.fileId, out FileItem fitem))
                                                {
                                                    pi.ContentUrls.Add(fitem.url);
                                                    var fn = $"{fitem.name}.{fitem.extension}";
                                                    pi.FileNames.Add(fn);
                                                    pi.Comments.Add($"<文件: {fn} ({GetSize(fitem.size)})>");
                                                }
                                                break;
                                            case "image":
                                                if (null != po.body.imageMap && po.body.imageMap.TryGetValue(binfo.imageId, out ImageItem iitem))
                                                {
                                                    pi.MediaUrls.Add(iitem.originalUrl);
                                                    var fn = $"{index_pic++}.{iitem.extension}";
                                                    pi.MediaNames.Add(fn);
                                                    pi.Comments.Add($"<图片: {fn} ({iitem.width}x{iitem.height}px)>");
                                                }
                                                break;
                                            case "embed":
                                                if (null != po.body.embedMap && po.body.embedMap.TryGetValue(binfo.embedId, out EmbedItem eitem))
                                                {
                                                    pi.Comments.Add(string.Empty);
                                                    if (eitem.serviceProvider == "twitter" && !string.IsNullOrEmpty(GlobalData.VM_MA.Artist.Twitter))
                                                    {
                                                        pi.Comments.Add($"<引用链接: {GlobalData.VM_MA.Artist.Twitter}/{eitem.contentId} >");
                                                    }
                                                    else if (eitem.serviceProvider == "fanbox")
                                                    {

                                                        pi.Comments.Add($"<引用链接: {GlobalData.VM_MA.Artist.PostUrl}/posts/{eitem.contentId.Split('/').Last()} >");
                                                    }
                                                    else
                                                    {
                                                        pi.Comments.Add($"<引用链接: {eitem.serviceProvider} ({eitem.contentId})>");
                                                    }
                                                    pi.Comments.Add(string.Empty);
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                            case "text":
                                {
                                    if (!string.IsNullOrEmpty(po.body.text))
                                    {
                                        pi.Comments.Add(po.body.text);
                                    }
                                }
                                break;
                        }
                    }
                    bis.Add(pi);
                }
                return fd.body.nextUrl ?? null;
            }
            return null;
        }

        public async override Task<ResultMessage> LikePost(string pid, string cid)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(_x_csrf_token))
                {
                    if (!UpdateToken(pid, cid))
                    {
                        return ResultHelper.SecurityError("");
                    }
                }
                try
                {
                    WebClient wc = new WebClient();
                    wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                    wc.Headers.Set(HttpRequestHeader.ContentType, "application/json");
                    wc.Headers.Set("Origin", "https://www.fanbox.cc");
                    wc.Headers.Add(HttpRequestHeader.Referer, $"https://www.fanbox.cc/@{cid}/posts/{pid}");
                    wc.Headers.Add("x-csrf-token", _x_csrf_token);
                    if (GlobalData.VM_MA.UseProxy)
                        wc.Proxy = GlobalData.VM_MA.MyProxy;

                    wc.UploadString("https://api.fanbox.cc/post.likePost", $"{{\"postId\":\"{pid}\"}}");

                    return ResultHelper.NoError(null);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("400"))
                    {
                        if (!UpdateToken(pid, cid))
                        {
                            return ResultHelper.SecurityError("");
                        }
                    }
                    return ResultHelper.WebError();
                }
            });
        }

        public bool UpdateToken(string pid, string cid)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                wc.Headers.Set("Origin", "https://www.fanbox.cc");
                wc.Headers.Add(HttpRequestHeader.Referer, $"https://www.fanbox.cc/@{cid}");
                if(GlobalData.VM_MA.UseProxy)
                    wc.Proxy = GlobalData.VM_MA.MyProxy;
                var shtml = wc.DownloadData($"https://www.fanbox.cc/@{cid}/posts/{pid}");
                Match ma = _csrfToken.Match(Encoding.UTF8.GetString(shtml));
                if (ma.Success)
                {
                    _x_csrf_token = ma.Groups[1].Value;
                    return true;
                }
            }
            catch { }
            return false;
        }

        private string GetWebCode(string url, string referer = null, string orig = "https://www.fanbox.cc")
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";

                if (GlobalData.VM_MA.UseProxy)
                    req.Proxy = GlobalData.VM_MA.MyProxy;

                req.Accept = "application/json, text/plain, */*";
                req.Headers.Set("Origin", orig);
                req.Headers.Set(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                req.Referer = referer ?? _referer;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();//获取返回结果
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);//以UTF8标准读取流
                string respHtml = sr.ReadToEnd();

                sr.Close();
                resp.Close();
                req.Abort();
                return respHtml;
            }
            catch
            {
                throw;
            }
        }

        private string GetSize(int size)
        {
            if (size <= 0)
            {
                return "未知大小";
            }
            var re = size / 1024d;
            var dw = "KB";
            if (re >= 1024)
            {
                re /= 1024d;
                dw = "MB";
                if (re >= 1024)
                {
                    re /= 1024d;
                    dw = "GB";
                }
            }
            return $"{Math.Round(re)}{dw}";
        }
    }
}
