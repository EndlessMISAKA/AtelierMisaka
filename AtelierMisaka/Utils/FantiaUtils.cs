using AtelierMisaka.Models;
using Newtonsoft.Json;
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
        readonly Regex _artIdName = GlobalRegex.GetRegex(RegexType.FantiaIdName);
        readonly Regex _artPlan = GlobalRegex.GetRegex(RegexType.FantiaPlan);
        readonly Regex _artPost = GlobalRegex.GetRegex(RegexType.FantiaPostId);
        readonly Regex _artUrl = GlobalRegex.GetRegex(RegexType.FantiaUrl);
        readonly Regex _artDataImage = GlobalRegex.GetRegex(RegexType.FantiaDataImage);
        readonly string _nextP = "fa fa-angle-right";
        private HashSet<string> _pidList = null;

        public async override Task<ResultMessage> GetArtistInfo(string url)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Match ma = _artUrl.Match(url);
                    if (!ma.Success)
                    {
                        return ResultHelper.PathError();
                    }
                    string cid = ma.Groups[1].Value;

                    var jfa = JsonConvert.DeserializeObject<JsonData_Fantia_Artist>(GetWebCode($"https://fantia.jp/api/v1/fanclubs/{cid}"));
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
                    List<ArtistInfo> ais = new List<ArtistInfo>();
                    //有料
                    ais.AddRange(GetArtistListFromWebCode("not_"));
                    //無料
                    ais.AddRange(GetArtistListFromWebCode(string.Empty));

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
                    if (ex is WebException || ex is System.Net.Sockets.SocketException)
                    {
                        return ex.Message.Contains("40") ? ResultHelper.CookieError() : ResultHelper.WebError();
                    }
                    return ResultHelper.UnKnownError();
                }
            });
        }

        private List<ArtistInfo> GetArtistListFromWebCode(string free, int index = 1)
        {
            try
            {
                List<ArtistInfo> ais = new List<ArtistInfo>();
                string sphtml = GetWebCode($"https://fantia.jp/mypage/users/plans?page={index}&type={free}free");
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
                    ais.AddRange(GetArtistListFromWebCode(free, index));
                }
                return ais;
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
                if(GlobalData.VM_MA.UseProxy)
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
                try
                {
                    _pidList = new HashSet<string>();
                    var bis = GetPostIDsFromWebCode(uid);
                    return ResultHelper.NoError(bis);
                }
                catch (Exception ex)
                {
                    GlobalMethord.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + "-----------------------------------------------");
                    if (ex is WebException || ex is System.Net.Sockets.SocketException)
                    {
                        return ex.Message.Contains("40") ? ResultHelper.CookieError() : ResultHelper.WebError();
                    }
                    else if (ex is InvalidDataException)
                    {
                        return ResultHelper.UnKnownError("Post Error: " + ex.Message);
                    }
                    return ResultHelper.UnKnownError();
                }
            });
        }

        private IList<BaseItem> GetPostIDsFromWebCode(string uid, int index = 1)
        {
            try
            {
                List<BaseItem> bis = new List<BaseItem>();
                string sphtml = GetWebCode($"https://fantia.jp/fanclubs/{uid}/posts?page={index}&utf8=%E2%9C%93&q%5Bs%5D=newer");
                Match ma = _artPost.Match(sphtml);
                bool flag = true;
                while (ma.Success)
                {
                    string pid = ma.Groups[1].Value;
                    if (_pidList.Add(pid))
                    {
                        var res = GetUrls(pid, bis);
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
                    bis.AddRange(GetPostIDsFromWebCode(uid, index));
                }
                return bis;
            }
            catch
            {
                throw;
            }
        }

        private bool? GetUrls(string pid, List<BaseItem> bis)
        {
            try
            {
                var jfp = JsonConvert.DeserializeObject<JsonData_Fantia_Post>(GetWebCode($"https://fantia.jp/api/v1/posts/{pid}"));
                //var jfp = JsonConvert.DeserializeObject<JsonData_Fantia_Post>(pid); //for test
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
                            else if(ct.category == "blog")
                            {
                                try
                                {
                                    JObject dd = JsonConvert.DeserializeObject(ct.comment) as JObject;
                                    JArray ja = JArray.Parse(dd["ops"].ToString());

                                    foreach (var js in ja)
                                    {
                                        var ss = js.SelectToken("insert");
                                        dynamic stem = ss;
                                        if (ss.Type == JTokenType.String)
                                        {
                                            fi.Comments.Add(stem.Value.Replace("\\n", Environment.NewLine));
                                        }
                                        else if(ss.Type == JTokenType.Object)
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
                        System.Threading.Thread.Sleep(2000);
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
            return await Task.Run(() =>
            {
                try
                {
                    var jfp = JsonConvert.DeserializeObject<JsonData_Fantia_Post>(GetWebCode($"https://fantia.jp/api/v1/posts/{pid}"));
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
                                        JObject dd = JsonConvert.DeserializeObject(ct.comment) as JObject;
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
                                    catch
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                        return fi;
                    }
                    return null;
                }
                catch
                {
                    throw;
                }
            });
        }

        private string GetWebCode(string url)
        {
            WebClient wc = new WebClient();
            try
            {
                wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                if (GlobalData.VM_MA.UseProxy)
                {
                    wc.Proxy = GlobalData.VM_MA.MyProxy;
                }
                var ss = wc.DownloadData(url);
                string s = Encoding.UTF8.GetString(ss);
                return s;
            }
            catch
            {
                throw;
            }
            finally
            {
                wc.Dispose();
            }
        }

        public async override Task<ResultMessage> LikePost(string pid, string cid)
        {
            return await Task.Run(() => ResultHelper.UnKnownError(GlobalLanguage.Msg_ErrorUnSupported));
        }
    }
}
