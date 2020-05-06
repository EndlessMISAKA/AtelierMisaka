using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Animation;

namespace AtelierMisaka.Views
{
    /// <summary>
    /// Pop_Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Pop_Setting : UserControl
    {
        private bool _mouseD = false;
        private Point _mouM = new Point(0, 0);
        //private bool _isNew = true;
        private bool _isOverDropBtn = false;
        private bool? _checkResult = null;
        //private bool _isStarted = false;
        private BaseUtils _utils = null;
        private Storyboard _sbLoad = null;

        private ArtistInfo _tempArt = null;
        private SiteType _tempSite = SiteType.Fanbox;
        private string _tempProxy = string.Empty;
        private string _tempCookies = string.Empty;
        private string _tempDate = string.Empty;
        private string _tempSP = string.Empty;
        private bool _tempUP = false;
        private bool _tempUD = false;

        private IList<BaseItem> _tempBis = null;

        public Pop_Setting()
        {
            InitializeComponent();
            _sbLoad = (Storyboard)TryFindResource("sb_path");
            LoadSetting();
        }

        private async void Btn_Star_Click(object sender, RoutedEventArgs e)
        {
            if (!GlobalData.VM_MA.IsStarted)
            {
                if (await GetCheck("确认无误就开始咯?"))
                {
                    if (_checkResult == false)
                    {
                        _checkResult = null;
                        return;
                    }
                    _checkResult = null;
                }
                ShowLoading(true);

                if (string.IsNullOrEmpty(GlobalData.VM_MA.Cookies))
                {
                    await GetCheck("Cookies不能为空");
                    _checkResult = null;
                    ShowLoading(false);
                    return;
                }

                if (string.IsNullOrEmpty(GlobalData.VM_MA.SavePath))
                {
                    await GetCheck("保存路径不能为空");
                    _checkResult = null;
                    ShowLoading(false);
                    return;
                }
                Directory.CreateDirectory(GlobalData.VM_MA.SavePath);
                if (!Directory.Exists(GlobalData.VM_MA.SavePath))
                {
                    await GetCheck("无法找到存储路径");
                    _checkResult = null;
                    ShowLoading(false);
                    return;
                }
                
                _utils = GlobalData.GetUtils();
                if (!GlobalData.VM_MA.HasSelected)
                {
                    var ai = await Task.Run(() => _utils.GetArtistInfos(GlobalData.VM_MA.Artist.PostUrl));
                    if (ai == null)
                    {
                        await GetCheck("无法获取作者信息", "请检查Cookies是否过期");
                        _checkResult = null;
                        ShowLoading(false);
                        return;
                    }
                    GlobalData.VM_MA.Artist = ai;
                    //GlobalData.VM_MA.Artist.Id = ai.Id;
                    //GlobalData.VM_MA.Artist.Cid = ai.Cid;
                    //GlobalData.VM_MA.Artist.AName = ai.AName;
                    //GlobalData.VM_MA.Artist.PostUrl = ai.PostUrl;
                    if (!GlobalData.VM_MA.ArtistList.Contains(GlobalData.VM_MA.Artist))
                    {
                        if (GlobalData.VM_MA.ArtistList.Count > 0)
                        {
                            GlobalData.VM_MA.ArtistList[GlobalData.VM_MA.ArtistList.Count - 1] = GlobalData.VM_MA.Artist;
                        }
                        else
                        {
                            GlobalData.VM_MA.ArtistList.Add(GlobalData.VM_MA.Artist);
                        }
                    }
                    SetLastDate(ai.Id);
                }
                Task.Run(() => SaveSetting());
                GlobalData.DownloadLogs = await Task.Run(() => GlobalData.Dbl.GetLog(GlobalData.VM_MA.Artist.Id));
                var ret = await Task.Run(() => _utils.GetPostIDs(GlobalData.VM_MA.Artist.Cid, out _tempBis));
                if (ret != ErrorType.NoError)
                {
                    if (ret == ErrorType.Web)
                    {
                        await GetCheck("发生网络错误", "请确认网络连接");
                    }
                    else if (ret == ErrorType.Cookies)
                    {
                        await GetCheck("请更换Cookies");
                    }
                    else if (ret == ErrorType.IO)
                    {
                        await GetCheck("磁盘读写出错");
                    }
                    else
                    {
                        await GetCheck("发生未知错误");
                    }
                    _checkResult = null;
                    ShowLoading(false);
                    return;
                }
                if (_tempBis == null || _tempBis.Count == 0)
                {
                    await GetCheck("没有可阅读的文章");
                    _checkResult = null;
                    ShowLoading(false);
                    return;
                }
                GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
                GlobalData.DownLP?.Close();
                GlobalData.DownLP = new Downloader(_tempBis, GlobalData.VM_MA.SavePath);
                GlobalData.DownLP.Show();
                GlobalData.DownLP.LoadData();
                GlobalData.BackCommand.Execute(BackType.Pop);

                _tempArt = new ArtistInfo()
                {
                    Id = GlobalData.VM_MA.Artist.Id,
                    AName = GlobalData.VM_MA.Artist.AName,
                    PayLow = GlobalData.VM_MA.Artist.PayLow,
                    PayHigh = GlobalData.VM_MA.Artist.PayHigh,
                    PostUrl = GlobalData.VM_MA.Artist.PostUrl,
                };
                _tempSite = GlobalData.VM_MA.Site;
                _tempCookies = GlobalData.VM_MA.Cookies;
                _tempProxy = GlobalData.VM_MA.Proxy;
                _tempDate = GlobalData.VM_MA.Date;
                _tempSP = GlobalData.VM_MA.SavePath;
                _tempUP = GlobalData.VM_MA.UseProxy;
                _tempUD = GlobalData.VM_MA.UseDate;

                ShowLoading(false);
                GlobalData.VM_MA.IsStarted = true;
            }
            else
            {
                if (await GetCheck("确认更改设定吗?"))
                {
                    if (_checkResult == false)
                    {
                        _checkResult = null;
                        return;
                    }
                    _checkResult = null;
                }
                ShowLoading(true);
                if (_tempSite != GlobalData.VM_MA.Site || _tempArt.PostUrl != GlobalData.VM_MA.Artist.PostUrl)
                {
                    if (GlobalData.VM_DL.IsDownloading)
                    {
                        await GetCheck("当前还在下载中，不能更换");
                        _checkResult = null;
                        ShowLoading(false);
                        return;
                    }
                    if (string.IsNullOrEmpty(GlobalData.VM_MA.Cookies))
                    {
                        await GetCheck("Cookies不能为空");
                        _checkResult = null;
                        ShowLoading(false);
                        return;
                    }
                    if (string.IsNullOrEmpty(GlobalData.VM_MA.SavePath))
                    {
                        await GetCheck("保存路径不能为空");
                        _checkResult = null;
                        ShowLoading(false);
                        return;
                    }
                    Directory.CreateDirectory(GlobalData.VM_MA.SavePath);
                    if (!Directory.Exists(GlobalData.VM_MA.SavePath))
                    {
                        await GetCheck("无法找到存储路径");
                        _checkResult = null;
                        ShowLoading(false);
                        return;
                    }
                    _utils = GlobalData.GetUtils();
                    if (!GlobalData.VM_MA.HasSelected)
                    {
                        var ai = await Task.Run(() => _utils.GetArtistInfos(GlobalData.VM_MA.Artist.PostUrl));
                        if (ai == null)
                        {
                            await GetCheck("无法获取作者信息");
                            _checkResult = null;
                            ShowLoading(false);
                            return;
                        }
                        GlobalData.VM_MA.Artist.Id = ai.Id;
                        GlobalData.VM_MA.Artist.Cid = ai.Cid;
                        GlobalData.VM_MA.Artist.AName = ai.AName;
                        GlobalData.VM_MA.Artist.PostUrl = $"https://{ai.Cid}.fanbox.cc";
                        if (!GlobalData.VM_MA.ArtistList.Contains(GlobalData.VM_MA.Artist))
                        {
                            if (GlobalData.VM_MA.ArtistList.Count > 0)
                            {
                                GlobalData.VM_MA.ArtistList[GlobalData.VM_MA.ArtistList.Count - 1] = GlobalData.VM_MA.Artist;
                            }
                            else
                            {
                                GlobalData.VM_MA.ArtistList.Add(GlobalData.VM_MA.Artist);
                            }
                        }
                        SetLastDate(ai.Id);
                    }
                    Task.Run(() => SaveSetting());
                    GlobalData.DownloadLogs = await Task.Run(() => GlobalData.Dbl.GetLog(GlobalData.VM_MA.Artist.Id));
                    var ret = await Task.Run(() => _utils.GetPostIDs(GlobalData.VM_MA.Artist.Cid, out _tempBis));
                    if (ret != ErrorType.NoError)
                    {
                        if (ret == ErrorType.Web)
                        {
                            await GetCheck("发生网络错误", "请确认网络连接");
                        }
                        else if (ret == ErrorType.Cookies)
                        {
                            await GetCheck("请更换Cookies");
                        }
                        else if (ret == ErrorType.IO)
                        {
                            await GetCheck("磁盘读写出错");
                        }
                        else
                        {
                            await GetCheck("发生未知错误");
                        }
                        _checkResult = null;
                        ShowLoading(false);
                        return;
                    }
                    if (_tempBis == null || _tempBis.Count == 0)
                    {
                        await GetCheck("没有可阅读的文章");
                        _checkResult = null;
                        ShowLoading(false);
                        return;
                    }
                    GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
                    GlobalData.DownLP?.Close();
                    GlobalData.DownLP = new Downloader(_tempBis, GlobalData.VM_MA.SavePath);
                    GlobalData.DownLP.Show();
                    GlobalData.DownLP.LoadData();
                    GlobalData.BackCommand.Execute(BackType.Pop);
                    _tempArt = new ArtistInfo()
                    {
                        Id = GlobalData.VM_MA.Artist.Id,
                        AName = GlobalData.VM_MA.Artist.AName,
                        PayLow = GlobalData.VM_MA.Artist.PayLow,
                        PayHigh = GlobalData.VM_MA.Artist.PayHigh,
                        PostUrl = GlobalData.VM_MA.Artist.PostUrl,
                    };
                    _tempSite = GlobalData.VM_MA.Site;
                    _tempCookies = GlobalData.VM_MA.Cookies;
                    _tempProxy = GlobalData.VM_MA.Proxy;
                    _tempDate = GlobalData.VM_MA.Date;
                    _tempSP = GlobalData.VM_MA.SavePath;

                    ShowLoading(false);
                    return;
                }

                if (_tempArt.PayLow != GlobalData.VM_MA.Artist.PayLow || _tempArt.PayHigh != GlobalData.VM_MA.Artist.PayHigh)
                {
                    await Task.Run(() =>
                    {
                        foreach (var bi in _tempBis)
                        {
                            bi.Skip = GlobalData.OverPayment(int.Parse(bi.Fee));
                        }
                    });
                    GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
                    _tempArt.PayLow = GlobalData.VM_MA.Artist.PayLow;
                    _tempArt.PayHigh = GlobalData.VM_MA.Artist.PayHigh;
                }

                if (_tempSP != GlobalData.VM_MA.SavePath)
                {
                    await GetCheck("保存路径的修改", "将在下次生效");
                    _checkResult = null;
                    _tempSP = GlobalData.VM_MA.SavePath;
                }

                if (_tempProxy != GlobalData.VM_MA.Proxy)
                {
                    GlobalData.VM_DL.DLClients.ForEach(x =>
                    {
                        GlobalData.VM_DL.ReStartCommand.Execute(x);
                    });
                    _tempProxy = GlobalData.VM_MA.Proxy;
                }

                if (_tempUP != GlobalData.VM_MA.UseProxy)
                {
                    GlobalData.VM_DL.DLClients.ForEach(x =>
                    {
                        GlobalData.VM_DL.ReStartCommand.Execute(x);
                    });
                    _tempUP = GlobalData.VM_MA.UseProxy;
                }

                if (_tempDate != GlobalData.VM_MA.Date || _tempUD != GlobalData.VM_MA.UseDate)
                {
                    await Task.Run(() =>
                    {
                        foreach (var bi in _tempBis)
                        {
                            bi.Skip = GlobalData.OverTime(bi.UpdateDate);
                        }
                    });
                    GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
                    #region old
                    /*
                    DateTime dt = DateTime.Parse(_tempDate);
                    DateTime dp = DateTime.Parse(GlobalData.VM_MA.Date);
                    if (dt > dp)
                    {
                        for (int i = 0; i < _tempBis.Count; i++)
                        {
                            if (_tempBis[i].Skip)
                            {
                                _tempBis[i].Skip = false;
                                DownloadItem di = null;
                                string ct = _tempBis[i].CreateDate.ToString("yyyyMMdd_HHmm");
                                string sp = $"{GlobalData.VM_DL.SavePath}\\{_tempBis[i].CreateDate.ToString("yyyyMMdd_HHmm")}_{(_tempBis[i].Title.Length > 20 ? _tempBis[i].Title.Substring(0, 20) : _tempBis[i].Title)}";
                                if (!string.IsNullOrEmpty(_tempBis[i].CoverPic))
                                {
                                    di = new DownloadItem
                                    {
                                        FileName = $"Cover.{_tempBis[i].CoverPic.Split('.').Last()}",
                                        Link = _tempBis[i].CoverPic,
                                        SavePath = sp,
                                        CTime = _tempBis[i].CreateDate
                                    };
                                    GlobalData.VM_DL.DownLoadItemList.Add(di);
                                }
                                for (int j = 0; j < _tempBis[i].ContentUrls.Count; j++)
                                {
                                    di = new DownloadItem
                                    {
                                        FileName = _tempBis[i].FileNames[j],
                                        Link = _tempBis[i].ContentUrls[j],
                                        SavePath = sp,
                                        CTime = _tempBis[i].CreateDate
                                    };
                                    GlobalData.VM_DL.DownLoadItemList.Add(di);
                                }
                                for (int k = 0; k < _tempBis[i].MediaUrls.Count; k++)
                                {
                                    di = new DownloadItem
                                    {
                                        FileName = _tempBis[i].MediaNames[k],
                                        Link = _tempBis[i].MediaUrls[k],
                                        SavePath = sp,
                                        CTime = _tempBis[i].CreateDate
                                    };
                                    GlobalData.VM_DL.DownLoadItemList.Add(di);
                                }
                                Directory.CreateDirectory(sp);
                                if (_tempBis[i].Comments.Count > 0)
                                {
                                    File.WriteAllLines(System.IO.Path.Combine(sp, "Comment.txt"), _tempBis[i].Comments);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = GlobalData.VM_DL.DownLoadItemList.Count - 1; i >= 0; i++)
                        {
                            if (GlobalData.VM_DL.DownLoadItemList[i].CTime < dt)
                            {
                                if (GlobalData.VM_DL.DownLoadItemList[i].DLStatus == DownloadStatus.Downloading)
                                {
                                    GlobalData.VM_DL.DownLoadItemList[i].DClient.CancelAsync();
                                }
                                else if (GlobalData.VM_DL.DownLoadItemList[i].DLStatus == DownloadStatus.Waiting)
                                {
                                    GlobalData.VM_DL.DownLoadItemList[i].DLStatus = DownloadStatus.Cancel;
                                }
                            }
                        }
                    }
                    /**/
                    #endregion
                    _tempDate = GlobalData.VM_MA.Date;
                    _tempUD = GlobalData.VM_MA.UseDate;
                }

                if (_tempCookies != GlobalData.VM_MA.Cookies)
                {
                    GlobalData.VM_DL.DLClients.ForEach(x =>
                    {
                        GlobalData.VM_DL.ReStartCommand.Execute(x);
                    });
                    _tempCookies = GlobalData.VM_MA.Cookies;
                }

                ShowLoading(false);
            }
            GlobalData.BackCommand.Execute(BackType.Pop);
        }

        private async void Btn_GetList_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GlobalData.VM_MA.Cookies))
            {
                await GetCheck("Cookies不能为空");
                _checkResult = null;
                return;
            }
            ShowLoading(true);
            _utils = GlobalData.GetUtils();
            var ais = await Task.Run(() =>
            {
                return _utils.GetArtistList();
            });
            GlobalData.VM_MA.ArtistList = new System.Collections.ObjectModel.ObservableCollection<ArtistInfo>();
            foreach (var ai in ais)
            {
                GlobalData.VM_MA.ArtistList.Add(ai);
            }
            GlobalData.VM_MA.Artist = GlobalData.VM_MA.ArtistList.Last();
            await Task.Run(() => SaveSetting());
            ShowLoading(false);
        }

        private async void Btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (await GetCheck("退出软件吗?"))
            {
                if (_checkResult == false)
                {
                    _checkResult = null;
                    return;
                }
                Application.Current.Shutdown();
            }
        }

        private void Btn_Check_Click(object sender, RoutedEventArgs e)
        {
            _checkResult = true;
            GlobalData.VM_MA.ShowCheck = false;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            _checkResult = false;
            GlobalData.VM_MA.ShowCheck = false;
        }

        private async Task<bool> GetCheck(params string[] msgs)
        {
            string mss = msgs[0];
            for (int i = 1; i < msgs.Length; i++)
            {
                mss += Environment.NewLine;
                mss += msgs[i];
            }
            GlobalData.VM_MA.Messages = mss;

            GlobalData.VM_MA.ShowCheck = true;
            GlobalData.VM_MA.MLeft = (cas.ActualWidth - 400) / 2;
            GlobalData.VM_MA.MTop = (cas.ActualHeight - 300) / 2;
            while (GlobalData.VM_MA.ShowCheck)
            {
                await Task.Delay(500);
            }
            return true;
        }

        private void ShowLoading(bool flag)
        {
            GlobalData.VM_MA.ShowLoad = flag;
            if (flag)
            {
                _sbLoad?.Begin();
            }
            else
            {
                _sbLoad?.Stop();
            }
        }

        private void SaveSetting()
        {
            try
            {
                string fn = $"Artists_{GlobalData.VM_MA.Site.ToString()}.json";
                File.WriteAllText(fn, GlobalData.ConverToJson(GlobalData.VM_MA.ArtistList));
                fn = $"Setting_{GlobalData.VM_MA.Site.ToString()}.ini";
                File.WriteAllLines(fn, new string[] { GlobalData.VM_MA.Cookies, GlobalData.VM_MA.SavePath, GlobalData.VM_MA.Proxy, GlobalData.VM_MA.UseProxy.ToString() });
                if (!string.IsNullOrEmpty(GlobalData.VM_MA.ArtistList.Last().Id))
                {
                    Dispatcher.Invoke(() => GlobalData.VM_MA.ArtistList.Add(new ArtistInfo()));
                }
            }
            catch
            {

            }
        }

        private void LoadSetting()
        {
            try
            {
                GlobalData.VM_MA.Site = SiteType.Patreon;
                var ais = GlobalData.ReadArtists("Artists_Patreon.json");
                foreach (var ai in ais)
                {
                    GlobalData.VM_MA.ArtistList.Add(ai);
                }
                if (File.Exists("Setting_Patreon.ini"))
                {
                    var ms = File.ReadAllLines("Setting_Patreon.ini");
                    GlobalData.VM_MA.Cookies = ms[0];
                    GlobalData.VM_MA.SavePath = ms[1];
                    GlobalData.VM_MA.Proxy = ms[2];
                    GlobalData.VM_MA.UseProxy = bool.Parse(ms[3]);
                }
                GlobalData.VM_MA.Site = SiteType.Fantia;
                ais = GlobalData.ReadArtists("Artists_Fantia.json");
                foreach (var ai in ais)
                {
                    GlobalData.VM_MA.ArtistList.Add(ai);
                }
                if (File.Exists("Setting_Fantia.ini"))
                {
                    var ms = File.ReadAllLines("Setting_Fantia.ini");
                    GlobalData.VM_MA.Cookies = ms[0];
                    GlobalData.VM_MA.SavePath = ms[1];
                    GlobalData.VM_MA.Proxy = ms[2];
                    GlobalData.VM_MA.UseProxy = bool.Parse(ms[3]);
                }
                GlobalData.VM_MA.Site = SiteType.Fanbox;
                ais = GlobalData.ReadArtists("Artists_Fanbox.json");
                foreach (var ai in ais)
                {
                    GlobalData.VM_MA.ArtistList.Add(ai);
                }
                if (File.Exists("Setting_Fanbox.ini"))
                {
                    var ms = File.ReadAllLines("Setting_Fanbox.ini");
                    GlobalData.VM_MA.Cookies = ms[0];
                    GlobalData.VM_MA.SavePath = ms[1];
                    GlobalData.VM_MA.Proxy = ms[2];
                    GlobalData.VM_MA.UseProxy = bool.Parse(ms[3]);
                }
                GlobalData.VM_MA.Artist = GlobalData.VM_MA.ArtistList.Last();
            }
            catch
            {

            }
        }

        private void Lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                ArtistInfo ai = (ArtistInfo)e.AddedItems[0];
                SetLastDate(ai.Id);
            }
            else
            {
                if (null == GlobalData.VM_MA.Artist && GlobalData.VM_MA.ArtistList.Count > 0)
                {
                    GlobalData.VM_MA.Artist = GlobalData.VM_MA.ArtistList.Last();
                }
            }
        }

        private void SetLastDate(string id)
        {
            if (GlobalData.Dbl.GetLastDate(id, out DateTime dt) == true)
            {
                GlobalData.VM_MA.Date = dt.ToString("yyyy/MM/dd HH:mm:ss");
                GlobalData.VM_MA.UseDate = true;
            }
            else
            {
                GlobalData.VM_MA.Date = string.Empty;
                GlobalData.VM_MA.UseDate = false;
            }
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (popup.IsOpen)
            {
                if (!popup.IsMouseCaptured && !_isOverDropBtn)
                {
                    popup.IsOpen = false;
                }
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (popup.IsOpen)
            {
                if (!popup.IsKeyboardFocused)
                {
                    popup.IsOpen = false;
                }
            }
        }

        private void Togb_MouseEnter(object sender, MouseEventArgs e)
        {
            _isOverDropBtn = true;
        }

        private void Togb_MouseLeave(object sender, MouseEventArgs e)
        {
            _isOverDropBtn = false;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseD = true;
            _mouM = e.GetPosition(cas);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseD)
            {
                var mm = e.GetPosition(cas);
                GlobalData.VM_MA.MLeft += (mm.X - _mouM.X);
                GlobalData.VM_MA.MTop += (mm.Y - _mouM.Y);
                _mouM = mm;
            }
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mouseD = false;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseD = false;
        }
    }
}
