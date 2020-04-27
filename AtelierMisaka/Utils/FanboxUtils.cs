using AtelierMisaka.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public override ErrorType GetPostIDs(string uid, out IList<BaseItem> bis)
        {
            bis = new List<BaseItem>();
            try
            {
                string url = $"https://api.fanbox.cc/post.listCreator?creatorId={uid}&limit=10";
                if (string.IsNullOrEmpty(_referer))
                {
                    _referer = $"https://{uid}.fanbox.cc";
                }
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";

                if (GlobalData.VM_MA.UseProxy)
                    req.Proxy = GlobalData.VM_MA.MyProxy;

                req.Accept = "application/json, text/plain, */*";
                req.Headers.Set("Origin", "https://www.fanbox.cc");
                req.Headers.Set(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                req.Referer = _referer;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();//获取返回结果
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);//以UTF8标准读取流
                string respHtml = sr.ReadToEnd();

                sr.Close();
                resp.Close();
                req.Abort();

                string nurl = GetUrls(respHtml, bis);
                if (!string.IsNullOrEmpty(nurl))
                {
                    GetPostIDs_Next(nurl, bis);
                }
                return ErrorType.NoError;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is System.Net.Sockets.SocketException)
                {
                    return ErrorType.Web;
                }
                else if (ex is IOException)
                {
                    return ErrorType.IO;
                }
                return ErrorType.UnKnown;
            }
        }

        protected override string GetUrls(string jsondata, IList<BaseItem> pis)
        {
            var jd = ConvertJSON(jsondata);
            if (null != jd)
            {
                var jts = jd.First.First.First.Children().Children();
                var jt = jts.First();
                int count = jts.Count();
                for (int po = 0; po < count; po++)
                {
                    var pi = new FanboxItem()
                    {
                        PID = jt["id"].ToString(),
                        Fee = jt["feeRequired"].ToString(),
                        Title = GlobalData.RemoveLastDot(GlobalData.ReplacePath(jt["title"].ToString().Trim())),
                        CreateDate = DateTime.Parse(jt["publishedDatetime"].ToString()),
                        UpdateDate = DateTime.Parse(jt["updatedDatetime"].ToString()),
                        CoverPic = jt["coverImageUrl"].ToString()
                    };
                    if (GlobalData.OverPayment(int.Parse(pi.Fee)) || GlobalData.OverTime(pi))
                    {
                        pi.Skip = true;
                    }
                    var ftype = jt["type"].ToString();
                    if (ftype == "file")
                    {
                        if (jt["body"].HasValues)
                        {
                            ftype += "s";
                            var comm = jt["body"]["text"];
                            if (comm != null)
                            {
                                pi.Comments.Add(comm.ToString());
                                pi.Comments.Add(Environment.NewLine);
                            }
                            var jtb = jt["body"][ftype].Children();
                            if (jtb.Count() > 0)
                            {
                                foreach (var fs in jtb)
                                {
                                    pi.ContentUrls.Add(fs["url"].ToString());
                                    var fn = $"{fs["name"].ToString()}.{fs["extension"].ToString()}";
                                    pi.FileNames.Add(fn);
                                    pi.Comments.Add($"文件: {fn}");
                                    pi.Comments.Add(Environment.NewLine);
                                }
                            }
                        }
                        pis.Add(pi);
                    }
                    else if (ftype == "image")
                    {
                        if (jt["body"].HasValues)
                        {
                            ftype += "s";
                            var comm = jt["body"]["text"];
                            if (comm != null)
                            {
                                pi.Comments.Add(comm.ToString());
                                pi.Comments.Add(Environment.NewLine);
                            }
                            var jtb = jt["body"][ftype].Children();
                            if (jtb.Count() > 0)
                            {
                                int index = 1;
                                foreach (var fs in jtb)
                                {
                                    pi.MediaUrls.Add(fs["originalUrl"].ToString());
                                    var fn = $"{index++}.{fs["extension"].ToString()}";
                                    pi.MediaNames.Add(fn);
                                    pi.Comments.Add($"图片: {fn}");
                                    pi.Comments.Add(Environment.NewLine);
                                }
                            }
                        }
                        pis.Add(pi);
                    }
                    else if (ftype == "article")
                    {
                        var jtb = jt["body"];
                        if (jtb.HasValues)
                        {
                            var sf = jtb["blocks"].First;
                            JEnumerable<JToken> jtbs = new JEnumerable<JToken>();
                            if (null != sf)
                            {
                                jtbs = jtb["blocks"].Children();
                            }
                            int index = 1;
                            if (jtbs.Count() > 0)
                            {
                                foreach (var fs in jtbs)
                                {
                                    var ttype = fs["type"].ToString();
                                    if (ttype == "p")
                                    {
                                        var tstr = fs["text"].ToString();
                                        pi.Comments.Add(tstr);
                                    }
                                    else if (ttype == "image")
                                    {
                                        var imgid = fs["imageId"].ToString();
                                        var imt = jtb["imageMap"][imgid];
                                        pi.Comments.Add($"<图{index}>");
                                        pi.MediaUrls.Add(imt["originalUrl"].ToString());
                                        pi.MediaNames.Add($"{index++}.{imt["extension"].ToString()}");
                                    }
                                    else if (ttype == "file")
                                    {
                                        var filid = fs["fileId"].ToString();
                                        var fit = jtb["fileMap"][filid];
                                        pi.Comments.Add($"<文件{index}>");
                                        pi.ContentUrls.Add(fit["url"].ToString());
                                        pi.FileNames.Add($"{fit["name"].ToString()}.{fit["extension"].ToString()}");
                                    }
                                }
                            }
                            else
                            {
                                sf = jtb["imageMap"].First;
                                if (sf != null)
                                {
                                    jtbs = jtb["blocks"].Children();
                                }
                                if (jtbs.Count() > 0)
                                {
                                    foreach (var fs in jtbs)
                                    {
                                        pi.MediaUrls.Add(fs["originalUrl"].ToString());
                                        pi.MediaNames.Add($"{index++}.{fs["extension"].ToString()}");
                                    }
                                }
                                else
                                {
                                    sf = jtb["fileMap"].First;
                                    if (sf != null)
                                    {
                                        jtbs = jtb["blocks"].Children();
                                    }
                                    if (jtbs.Count() > 0)
                                    {
                                        foreach (var fs in jtbs)
                                        {
                                            pi.ContentUrls.Add(fs["url"].ToString());
                                            pi.FileNames.Add($"{fs["name"].ToString()}.{fs["extension"].ToString()}");
                                        }
                                    }
                                }
                            }
                        }
                        pis.Add(pi);
                    }
                    jt = jt.Next;
                }
                var nul = jd.First.First["nextUrl"].ToString();
                return string.IsNullOrEmpty(nul) ? null : nul;
            }
            return null;
        }

        protected override void GetPostIDs_Next(string url, IList<BaseItem> pis)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";

                if (GlobalData.VM_MA.UseProxy)
                    req.Proxy = GlobalData.VM_MA.MyProxy;

                req.Accept = "application/json, text/plain, */*";
                req.Headers.Set("Origin", "https://www.fanbox.cc");
                req.Headers.Set(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                req.Referer = _referer;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();//获取返回结果
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);//以UTF8标准读取流
                string respHtml = sr.ReadToEnd();

                sr.Close();
                resp.Close();
                req.Abort();

                string nurl = GetUrls(respHtml, pis);
                if (!string.IsNullOrEmpty(nurl))
                {
                    GetPostIDs_Next(nurl, pis);
                }
            }
            catch
            {
                throw;
            }
        }

        public override ArtistInfo GetArtistInfos(string url)
        {
            try
            {
                Match ma = (new Regex(@"^https://(\w+)\.fanbox\.cc/?$")).Match(url);
                if (!ma.Success)
                {
                    ma = (new Regex(@"^https://www\.fanbox\.cc/@(\w+)$")).Match(url);
                    if (!ma.Success)
                    {
                        return null;
                    }
                    url = $"https://{ma.Groups[1].Value}.fanbox.cc/";
                }
                _referer = url;
                string cid = ma.Groups[1].Value;
                url = $"https://api.fanbox.cc/creator.get?creatorId={cid}";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);//请求数据
                req.Method = "GET";
                if (GlobalData.VM_MA.UseProxy)
                    req.Proxy = GlobalData.VM_MA.MyProxy;

                req.Accept = "application/json, text/plain, */*";
                req.Headers.Set(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                req.Headers.Set("Origin", "https://www.fanbox.cc");
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36";
                req.Referer = _referer;

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();//获取返回结果
                                                                          //otherwise will return messy code
                                                                          //  Encoding htmlEncoding = Encoding.GetEncoding(htmlCharset);
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);//以UTF8标准读取流
                                                                                            //read out the returned html
                string respHtml = sr.ReadToEnd();

                sr.Close();
                resp.Close();
                req.Abort();

                var jd = ConvertJSON(respHtml);
                if (null != jd)
                {
                    var _cid = jd.First.First["creatorId"].ToString();
                    var jj = jd.First.First.First.First;
                    var ai = new ArtistInfo()
                    {
                        Id = jj["userId"].ToString(),
                        AName = GlobalData.RemoveLastDot(GlobalData.ReplacePath(jj["name"].ToString().Trim())),
                        Cid = _cid
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
                string url = "https://api.fanbox.cc/plan.listSupporting";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);//请求数据
                req.Method = "GET";
                if (GlobalData.VM_MA.UseProxy)
                    req.Proxy = GlobalData.VM_MA.MyProxy;

                req.Accept = "application/json, text/plain, */*";
                req.Headers.Set(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                req.Headers.Set("Origin", "https://www.fanbox.cc");
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36";
                req.Referer = "https://www.fanbox.cc/creators/supporting";

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();//获取返回结果
                                                                          //otherwise will return messy code
                                                                          //  Encoding htmlEncoding = Encoding.GetEncoding(htmlCharset);
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);//以UTF8标准读取流
                                                                                            //read out the returned html
                string respHtml = sr.ReadToEnd();

                sr.Close();
                resp.Close();
                req.Abort();

                List<ArtistInfo> ais = new List<ArtistInfo>();
                var tais = GlobalData.VM_MA.ArtistList.ToList();
                if(tais.Count == 0)
                {
                    tais.Add(new ArtistInfo());
                }

                var jd = ConvertJSON(respHtml);
                if (null != jd)
                {
                    var jts = jd.First.Children();
                    var count = jts.Count();
                    if (count > 0)
                    {
                        var jt = jts.First().First;
                        do
                        {
                            ArtistInfo ai = new ArtistInfo();
                            ai.PayHigh = jt["fee"].ToString();
                            ai.Cid = jt["creatorId"].ToString();
                            var jtd = jt["user"];
                            ai.Id = jtd["userId"].ToString();
                            ai.AName = GlobalData.RemoveLastDot(GlobalData.ReplacePath(jtd["name"].ToString().Trim()));
                            ai.PostUrl = $"https://{ai.Cid}.fanbox.cc";

                            var index = tais.IndexOf(ai);
                            if (index != -1)
                            {
                                tais.RemoveAt(index);
                                //GlobalData.SyContext.Post(pl =>
                                //{
                                //    GlobalData.VM_MA.ArtistList.Add((ArtistInfo)pl);
                                //}, ai);
                            }
                            ais.Add(ai);
                            jt = jt.Next;
                        } while (jt != null);
                    }
                }
                ais.AddRange(tais);
                return ais;
            }
            catch (Exception ex)
            {
                return new List<ArtistInfo>() { new ArtistInfo() };
            }
        }

        public override bool GetCover(BaseItem bi)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                wc.Headers.Add(HttpRequestHeader.Referer, GlobalData.VM_MA.Artist.PostUrl);
                wc.Proxy = GlobalData.VM_MA.MyProxy;
                bi.ImgData = wc.DownloadData(bi.CoverPic);
                bi.NeedLoadCover = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
