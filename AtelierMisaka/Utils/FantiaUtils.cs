using AtelierMisaka.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AtelierMisaka
{
    public class FantiaUtils : BaseUtils
    {
        readonly Regex _artIdName = new Regex(@"/fanclubs/(\d+)""><strong>(.+?)</strong>");
        readonly Regex _artPlan = new Regex(@"(\d+)円/月\)</strong");
        readonly Regex _artPost = new Regex(@"block"" href=""/posts/(\d+)");
        readonly Regex _artUrl = new Regex(@"^https://fantia.jp/fanclubs/(\d+)$");
        readonly string _nextP = "fa fa-angle-right";

        public override ArtistInfo GetArtistInfo(string url)
        {
            try
            {
                Match ma = _artUrl.Match(url);
                if (!ma.Success)
                {
                    return null;
                }
                string cid = ma.Groups[1].Value;
                
                var jfa = JsonConvert.DeserializeObject<JsonData_Fantia_Artist>(GetWebCode($"https://fantia.jp/api/v1/fanclubs/{cid}"));
                if(null != jfa.fanclub)
                {
                    var ai = new ArtistInfo()
                    {
                        Id = cid,
                        AName = GlobalData.RemoveLastDot(GlobalData.ReplacePath(jfa.fanclub.creator_name)),
                        Cid = cid,
                        PostUrl = $"https://fantia.jp/fanclubs/{cid}",
                        PayLow = GlobalData.VM_MA.Artist.PayLow,
                        PayHigh = GlobalData.VM_MA.Artist.PayHigh
                    };
                    return ai;
                }
            }
            catch
            {

            }
            return null;
        }

        public override List<ArtistInfo> GetArtistList()
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
                return ais;
            }
            catch
            {
                return new List<ArtistInfo>() { new ArtistInfo() };
            }
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
                        AName = GlobalData.RemoveLastDot(GlobalData.ReplacePath(ana)),
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
                        ais[i++].PayHigh = ma.Groups[1].Value;
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
                return new List<ArtistInfo>();
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

        public override ErrorType GetPostIDs(string uid, out IList<BaseItem> bis)
        {
            try
            {
                bis = GetPostIDsFromWebCode(uid);
                return ErrorType.NoError;
            }
            catch (Exception ex)
            {
                bis = new List<BaseItem>();
                GlobalData.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace);
                if (ex is WebException || ex is System.Net.Sockets.SocketException)
                {
                    return ex.Message.Contains("401") ? ErrorType.Cookies : ErrorType.Web;
                }
                else if (ex is IOException)
                {
                    return ErrorType.IO;
                }
                return ErrorType.UnKnown;
            }
        }

        private IList<BaseItem> GetPostIDsFromWebCode(string uid)
        {
            try
            {
                List<BaseItem> bis = new List<BaseItem>();
                Match ma = _artPost.Match(GetWebCode($"https://fantia.jp/fanclubs/{uid}"));
                if (ma.Success)
                {
                    GetUrls(ma.Groups[1].Value, bis);
                }
                return bis;
            }
            catch
            {
                throw;
            }
        }

        private void GetUrls(string pid, List<BaseItem> bis)
        {
            try
            {
                var jfp = JsonConvert.DeserializeObject<JsonData_Fantia_Post>(GetWebCode($"https://fantia.jp/api/v1/posts/{pid}"));
                if (null != jfp.post)
				{
					if (GlobalData.OverTime(jfp.post.converted_at))
					{
						return;
					}
					FantiaItem fi = new FantiaItem()
                    {
                        CreateDate = jfp.post.posted_at,
                        UpdateDate = jfp.post.converted_at
                    };
                    fi.FID = jfp.post.id.ToString();
                    fi.Title = GlobalData.RemoveLastDot(GlobalData.ReplacePath(jfp.post.title));
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
                    if (DateTime.TryParse(jfp.post.deadline, out DateTime dt))
                    {
                        fi.DeadDate = dt.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    else
                    {
                        fi.DeadDate = "无";
                    }
					
                    foreach (var ct in jfp.post.post_contents)
                    {
                        var fee = 0;
                        if (null != ct.plan)
                        {
                            fee = ct.plan.price;
                        }
                        var stitle = $"${fee}___{GlobalData.RemoveLastDot(GlobalData.ReplacePath(ct.title))}";
                        fi.Comments.Add("------------------------------------------------------------------------------------------");
                        fi.Comments.Add(stitle);
                        fi.Comments.Add(string.Empty);
                        if (ct.visible_status == "visible")
                        {
                            if (!string.IsNullOrEmpty(ct.comment))
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
									fi.Comments.Add($"<图片: {fn}>");
									fi.FileNames.Add(fn);
                                    fi.ContentUrls.Add(imgUrl);
                                    fi.Fees.Add($"{fee}");
                                    fi.PTitles.Add(stitle);
                                }
                            }
                            else if (ct.category == "file")
							{
								fi.Comments.Add($"<文件: {ct.filename}>");
								fi.FileNames.Add(ct.filename);
                                fi.ContentUrls.Add($"https://fantia.jp{ct.download_uri}");
                                fi.Fees.Add($"{fee}");
                                fi.PTitles.Add(stitle);
                            }
                        }
                    }
                    bis.Add(fi);
                    if (null != jfp.post.links && null != jfp.post.links.previous)
                    {
                        if (!GlobalData.OverTime(jfp.post.links.previous.converted_at))
                        {
                            GetUrls_Loop(jfp.post.links.previous.id, bis);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void GetUrls_Loop(int pid, List<BaseItem> bis)
        {
            try
            {
                var jfp = JsonConvert.DeserializeObject<JsonData_Fantia_Post>(GetWebCode($"https://fantia.jp/api/v1/posts/{pid}"));
                if (null != jfp.post)
                {
                    FantiaItem fi = new FantiaItem()
                    {
                        FID = jfp.post.id.ToString(),
                        Title = GlobalData.RemoveLastDot(GlobalData.ReplacePath(jfp.post.title)),
                        CreateDate = jfp.post.posted_at,
                        UpdateDate = jfp.post.converted_at
                    };
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

                    var contents = jfp.post.post_contents;
                    foreach (var ct in contents)
                    {
                        var fee = 0;
                        if (null != ct.plan)
                        {
                            fee = ct.plan.price;
                        }
                        var stitle = $"${fee}___{GlobalData.RemoveLastDot(GlobalData.ReplacePath(ct.title))}";
                        fi.Comments.Add("------------------------------------------------------------------------------------------");
                        fi.Comments.Add(stitle);
                        fi.Comments.Add(string.Empty);
                        if (ct.visible_status == "visible")
                        {
                            if (!string.IsNullOrEmpty(ct.comment))
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
                                    fi.Comments.Add($"<图片: {fn}>");
                                    fi.FileNames.Add(fn);
                                    fi.ContentUrls.Add(imgUrl);
                                    fi.Fees.Add($"{fee}");
                                    fi.PTitles.Add(stitle);
                                }
                            }
                            else if (ct.category == "file")
                            {
                                fi.Comments.Add($"<文件: {ct.filename}>");
                                fi.FileNames.Add(ct.filename);
                                fi.ContentUrls.Add($"https://fantia.jp{ct.download_uri}");
                                fi.Fees.Add($"{fee}");
                                fi.PTitles.Add(stitle);
                            }
                        }
                    }
                    bis.Add(fi);
                    if (null != jfp.post.links && null != jfp.post.links.previous)
                    {
                        if (!GlobalData.OverTime(jfp.post.links.previous.converted_at))
                        {
                            GetUrls_Loop(jfp.post.links.previous.id, bis);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
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

        public override ErrorType LikePost(string pid, string cid)
        {
            return ErrorType.UnKnown;
        }
    }
}
