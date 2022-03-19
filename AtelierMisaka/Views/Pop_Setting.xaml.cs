using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace AtelierMisaka.Views
{
    /// <summary>
    /// Pop_Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Pop_Setting : UserControl
    {
        private bool _isOverDropBtn = false;
        private BaseUtils _utils = null;
        private Storyboard _sbLoad = null;

        private ArtistInfo _tempArt = null;
        private SiteType _tempSite = SiteType.Fanbox;
        private string _tempProxy = string.Empty;
        private string _tempCookies = string.Empty;
        private string _tempDate = string.Empty;
        private string _tempDate_end = string.Empty;
        private string _tempSP = string.Empty;
        private bool _tempUP = false;
        private bool _tempUD = false;

        private bool _second = false;
        private bool _selectF = true;

        private IList<BaseItem> _tempBis = null;

        public Pop_Setting()
        {
            InitializeComponent();
            _sbLoad = (Storyboard)TryFindResource("sb_path");
            LoadSetting();
        }

        private async void Btn_Star_Click(object sender, RoutedEventArgs e)
        {
            if (_second) return;
            _second = true;

            if (await GetCheck(GlobalData.VM_MA.IsStarted ? GlobalLanguage.Msg_SecondConf : GlobalLanguage.Msg_StartConf))
            {
                if (GlobalData.CheckResult == false)
                {
                    _second = false;
                    return;
                }
            }

            GlobalData.StartTime = DateTime.Now;
            if (string.IsNullOrEmpty(GlobalData.VM_MA.Date_End))
            {
                GlobalData.VM_MA.LastDate_End = DateTime.Now;
            }

            if (!GlobalData.VM_MA.IsStarted)
            {
                ShowLoading(true);
                await Task.Delay(200);
                if (await Begin())
                {
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
                    _tempDate = GlobalData.VM_MA.Date_Start;
                    _tempDate_end = GlobalData.VM_MA.Date_End;
                    _tempSP = GlobalData.VM_MA.SavePath;
                    _tempUP = GlobalData.VM_MA.UseProxy;
                    _tempUD = GlobalData.VM_MA.UseDate;

                    GlobalData.CurrentSite = _tempSite;
                    GlobalData.VM_MA.IsStarted = true;
                }
                ShowLoading(false);
            }
            else
            {
                ShowLoading(true);
                if (_tempSite != GlobalData.VM_MA.Site || _tempArt.PostUrl != GlobalData.VM_MA.Artist.PostUrl)
                {
                    if (GlobalData.VM_DL.IsDownloading)
                    {
                        await GetCheck(GlobalLanguage.Msg_IsDownload);
                        ShowLoading(false);
                        _second = false;
                        return;
                    }
                    
                    if (await Begin())
                    {
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
                        _tempDate = GlobalData.VM_MA.Date_Start;
                        _tempDate_end = GlobalData.VM_MA.Date_End;
                        _tempSP = GlobalData.VM_MA.SavePath;

                        GlobalData.CurrentSite = _tempSite;
                    }
                    ShowLoading(false);
                    _second = false;
                    return;
                }

                if (_tempArt.PayLow != GlobalData.VM_MA.Artist.PayLow || _tempArt.PayHigh != GlobalData.VM_MA.Artist.PayHigh)
                {
                    if (GlobalData.VM_MA.Site == SiteType.Fanbox)
                    {
                        await Task.Run(() =>
                        {
                            foreach (var bi in _tempBis)
                            {
                                bi.Skip = GlobalMethord.OverPayment(int.Parse(bi.Fee));
                            }
                        });
                        GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
                        _tempArt.PayLow = GlobalData.VM_MA.Artist.PayLow;
                        _tempArt.PayHigh = GlobalData.VM_MA.Artist.PayHigh;
                    }
                }

                if (_tempSP != GlobalData.VM_MA.SavePath)
                {
                    await GetCheck(GlobalLanguage.Msg_ChangeSP);
                    _tempSP = GlobalData.VM_MA.SavePath;
                }

                if (_tempProxy != GlobalData.VM_MA.Proxy)
                {
                    GlobalData.VM_DL.DLClients.ForEach(x =>
                    {
                        GlobalData.VM_DL.ReStartCommand.Execute(x);
                    });
                    if (null != GlobalData.VM_MA.PatreonCefBrowser)
                    {
                        await CefHelper.SetProxy((CefSharp.Wpf.ChromiumWebBrowser)GlobalData.VM_MA.PatreonCefBrowser, GlobalData.VM_MA.Proxy);
                    }
                    _tempProxy = GlobalData.VM_MA.Proxy;
                }

                if (_tempUP != GlobalData.VM_MA.UseProxy)
                {
                    GlobalData.VM_DL.DLClients.ForEach(x =>
                    {
                        GlobalData.VM_DL.ReStartCommand.Execute(x);
                    });
                    if (GlobalData.VM_MA.UseProxy && null != GlobalData.VM_MA.PatreonCefBrowser)
                    {
                        await CefHelper.SetProxy((CefSharp.Wpf.ChromiumWebBrowser)GlobalData.VM_MA.PatreonCefBrowser, GlobalData.VM_MA.Proxy);
                    }
                    _tempUP = GlobalData.VM_MA.UseProxy;
                }

                if (_tempCookies != GlobalData.VM_MA.Cookies)
                {
                    GlobalData.VM_DL.DLClients.ForEach(x =>
                    {
                        GlobalData.VM_DL.ReStartCommand.Execute(x);
                    });
                    _tempCookies = GlobalData.VM_MA.Cookies;
                }

                if (_tempDate != GlobalData.VM_MA.Date_Start || _tempDate_end != GlobalData.VM_MA.Date_End || _tempUD != GlobalData.VM_MA.UseDate)
                {
                    //if (GlobalData.VM_MA.Site == SiteType.Fanbox || CompareDate(GlobalData.VM_MA.Date_Start, _tempDate) || CompareDate(GlobalData.VM_MA.Date_End, _tempDate_end))
                    //{
                    //    await Task.Run(() =>
                    //    {
                    //        foreach (var bi in _tempBis)
                    //        {
                    //            bi.Skip = GlobalMethord.OverTime(bi.UpdateDate);
                    //        }
                    //    });
                    //    GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
                    //    _tempDate = GlobalData.VM_MA.Date_Start;
                    //    _tempDate_end = GlobalData.VM_MA.Date_End;
                    //    _tempUD = GlobalData.VM_MA.UseDate;
                    //}
                    //else
                    {
                        if (GlobalData.VM_DL.IsDownloading)
                        {
                            await GetCheck(GlobalLanguage.Msg_IsDownload);
                            ShowLoading(false);
                            _second = false;
                            return;
                        }

                        if (await Begin())
                        {
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
                            _tempDate = GlobalData.VM_MA.Date_Start;
                            _tempDate_end = GlobalData.VM_MA.Date_End;
                            _tempSP = GlobalData.VM_MA.SavePath;

                            GlobalData.CurrentSite = _tempSite;
                        }
                        ShowLoading(false);
                        _second = false;
                        return;
                    }
                }
                ShowLoading(false);
                GlobalCommand.BackCommand.Execute(BackType.Pop);
            }
            _second = false;
        }

        private async Task<bool> Begin()
        {
            if (string.IsNullOrEmpty(GlobalData.VM_MA.Cookies))
            {
                await GetCheck(GlobalLanguage.Msg_CheckCk);
                return false;
            }

            if (string.IsNullOrEmpty(GlobalData.VM_MA.SavePath))
            {
                await GetCheck(GlobalLanguage.Msg_CheckSP);
                return false;
            }
            Directory.CreateDirectory(GlobalData.VM_MA.SavePath);
            if (!Directory.Exists(GlobalData.VM_MA.SavePath))
            {
                await GetCheck(GlobalLanguage.Msg_CreateSP);
                return false;
            }
            GetSystemProxy();

            if (null != GlobalData.DownLP)
            {
                GlobalData.VM_DL.DownLoadItemList.Clear();
                GlobalData.DownLP.Close();
            }
            GlobalData.VM_MA.PostCount = 0;
            GlobalData.VM_MA.PostTitle = string.Empty;
            ResultMessage _ret = null;
            _utils = GlobalData.CaptureUtil;
            if (_utils is PatreonUtils)
            {
                _ret = await (_utils as PatreonUtils).InitBrowser();
                if (_ret.Error != ErrorType.NoError)
                {
                    await GetCheck(_ret.Msgs);
                    return false;
                }
                else if ((bool)_ret.Result)
                {
                    while (GlobalData.VM_MA.ShowLogin)
                    {
                        await Task.Delay(1000);
                    }
                    if (!GlobalData.VM_MA.IsInitialized)
                    {
                        return false;
                    }
                }
            }
            _selectF = GlobalData.VM_MA.HasSelected;
            if (!_selectF)
            {
                ArtistInfo ai = null;
                _ret = await _utils.GetArtistInfo(GlobalData.VM_MA.Artist.PostUrl);
                if (_ret.Error != ErrorType.NoError)
                {
                    await GetCheck(_ret.Msgs);
                    return false;
                }
                ai = (ArtistInfo)_ret.Result;
                GlobalData.VM_MA.Artist = ai;
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
            }
            Task.Run(() => SaveSetting());
            await Task.Run(() => GlobalData.DLLogs.LoadData(GlobalData.VM_MA.Artist.Id, GlobalData.VM_MA.Site));

            if (GlobalData.VM_MA.Site == SiteType.Fantia)
            {
                GlobalData.DownLP = new Downloader(null, true);
                GlobalData.DownLP.Show();
                GlobalData.DownLP.LoadData();
                _ret = await _utils.GetPostIDs(GlobalData.VM_MA.Artist.Cid);
                if (_ret.Error != ErrorType.NoError)
                {
                    await GetCheck(_ret.Msgs);
                    return false;
                }
                _tempBis = (List<BaseItem>)_ret.Result;
                if (_tempBis.Count == 0)
                {
                    await GetCheck(GlobalLanguage.Msg_NoPosts);
                    return false;
                }
                GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
                GlobalCommand.BackCommand.Execute(BackType.Pop);
            }
            else
            {
                _ret = await _utils.GetPostIDs(GlobalData.VM_MA.Artist.Cid);
                if (_ret.Error != ErrorType.NoError)
                {
                    await GetCheck(_ret.Msgs);
                    return false;
                }
                _tempBis = (List<BaseItem>)_ret.Result;
                if (_tempBis.Count == 0)
                {
                    await GetCheck(GlobalLanguage.Msg_NoPosts);
                    return false;
                }
                GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
                GlobalData.DownLP = new Downloader(_tempBis);
                GlobalData.DownLP.Show();
                GlobalData.DownLP.LoadData();
                GlobalCommand.BackCommand.Execute(BackType.Pop);
            }
            return true;
        }

        private void GetSystemProxy()
        {
            var proxyConfig = new WinHttpCurrentUserIEProxyConfig();
            Win32Utils.WinHttpGetIEProxyConfigForCurrentUser(ref proxyConfig);
            if (string.IsNullOrEmpty(proxyConfig.Proxy))
            {
                GlobalData.VM_MA.ProxySystem = string.Empty;
            }
            else
            {
                if (!proxyConfig.Proxy.Contains("="))
                {
                    GlobalData.VM_MA.ProxySystem = proxyConfig.Proxy;
                }
                else
                {
                    var settings = proxyConfig.Proxy.Split(';');
                    foreach (var setting in settings)
                    {
                        var groups = GlobalRegex.ProxyPattern.Match(setting).Groups;
                        if (groups.Count < 1) continue;
                        switch (groups["scheme"].Value)
                        {
                            case "http":
                                if (ushort.TryParse(groups["port"].Value, out var httpPort))
                                {
                                    GlobalData.VM_MA.ProxySystem = $"{groups["host"].Value}:{httpPort}";
                                }
                                return;
                            default:
                                break;
                        }
                    }
                    GlobalData.VM_MA.ProxySystem = string.Empty;
                }
            }
        }

        private async void Btn_GetList_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GlobalData.VM_MA.Cookies))
            {
                await GetCheck(GlobalLanguage.Msg_CheckCk);
                return;
            }
            ShowLoading(true);
            await Task.Delay(100);
            ResultMessage _ret = null;
            GetSystemProxy();
            _utils = GlobalData.CaptureUtil;
            if (_utils is PatreonUtils)
            {
                _ret = await (_utils as PatreonUtils).InitBrowser();
                if (_ret.Error != ErrorType.NoError)
                {
                    await GetCheck(_ret.Msgs);
                    ShowLoading(false);
                    return;
                }
                else if ((bool)_ret.Result)
                {
                    while (GlobalData.VM_MA.ShowLogin)
                    {
                        await Task.Delay(1000);
                    }
                    if (!GlobalData.VM_MA.IsInitialized)
                    {
                        ShowLoading(false);
                        return;
                    }
                }
            }
            _ret = await _utils.GetArtistList();
            if (_ret.Error != ErrorType.NoError)
            {
                await GetCheck(_ret.Msgs);
                ShowLoading(false);
                return;
            }
            List<ArtistInfo> ais = (List<ArtistInfo>)_ret.Result;
            GlobalData.VM_MA.ArtistList = new System.Collections.ObjectModel.ObservableCollection<ArtistInfo>();
            foreach (var ai in ais)
            {
                GlobalData.VM_MA.ArtistList.Add(ai);
            }
            GlobalData.VM_MA.Artist = GlobalData.VM_MA.ArtistList.Last();
            await Task.Run(() => SaveSetting());
            ShowLoading(false);
        }

        private void Btn_Back_Click(object sender, RoutedEventArgs e)
        {
            GlobalCommand.ExitCommand.Execute(null);
        }

        private bool CompareDate(string d1, string d2)
        {
            if (DateTime.TryParse(d1, out DateTime dt1))
            {
                if (DateTime.TryParse(d2, out DateTime dt2))
                {
                    return dt1 > dt2;
                }
            }
            return false;
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
            
            GlobalData.VM_MA.MLeft = (ActualWidth - 550) / 2;
            GlobalData.VM_MA.MTop = (ActualHeight - 300) / 2;
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
                string fn = $"Settings\\Artists_{GlobalData.VM_MA.Site.ToString()}.json";
                File.WriteAllText(fn, GlobalMethord.ConverToJson(GlobalData.VM_MA.ArtistList));
                fn = $"Settings\\Setting_{GlobalData.VM_MA.Site.ToString()}.ini";
                File.WriteAllLines(fn, new string[] { GlobalData.VM_MA.Cookies, GlobalData.VM_MA.SavePath, GlobalData.VM_MA.Proxy, GlobalData.VM_MA.UseProxy.ToString() });
                if (!string.IsNullOrEmpty(GlobalData.VM_MA.ArtistList.Last().Id))
                {
                    Dispatcher.Invoke(() => GlobalData.VM_MA.ArtistList.Add(new ArtistInfo()));
                }
            }
            catch { }
        }

        private void LoadSetting()
        {
            try
            {
                GlobalData.VM_MA.Site = SiteType.Patreon;
                var ais = GlobalMethord.ReadArtists("Settings\\Artists_Patreon.json");
                foreach (var ai in ais)
                {
                    GlobalData.VM_MA.ArtistList.Add(ai);
                }
                if (File.Exists("Settings\\Setting_Patreon.ini"))
                {
                    var ms = File.ReadAllLines("Settings\\Setting_Patreon.ini");
                    GlobalData.VM_MA.Cookies = ms[0];
                    GlobalData.VM_MA.SavePath = ms[1];
                    GlobalData.VM_MA.Proxy = ms[2];
                    GlobalData.VM_MA.UseProxy = bool.Parse(ms[3]);
                }
                GlobalData.VM_MA.Site = SiteType.Fantia;
                ais = GlobalMethord.ReadArtists("Settings\\Artists_Fantia.json");
                foreach (var ai in ais)
                {
                    GlobalData.VM_MA.ArtistList.Add(ai);
                }
                if (File.Exists("Settings\\Setting_Fantia.ini"))
                {
                    var ms = File.ReadAllLines("Settings\\Setting_Fantia.ini");
                    GlobalData.VM_MA.Cookies = ms[0];
                    GlobalData.VM_MA.SavePath = ms[1];
                    GlobalData.VM_MA.Proxy = ms[2];
                    GlobalData.VM_MA.UseProxy = bool.Parse(ms[3]);
                }
                GlobalData.VM_MA.Site = SiteType.Fanbox;
                ais = GlobalMethord.ReadArtists("Settings\\Artists_Fanbox.json");
                foreach (var ai in ais)
                {
                    GlobalData.VM_MA.ArtistList.Add(ai);
                }
                if (File.Exists("Settings\\Setting_Fanbox.ini"))
                {
                    var ms = File.ReadAllLines("Settings\\Setting_Fanbox.ini");
                    GlobalData.VM_MA.Cookies = ms[0];
                    GlobalData.VM_MA.SavePath = ms[1];
                    GlobalData.VM_MA.Proxy = ms[2];
                    GlobalData.VM_MA.UseProxy = bool.Parse(ms[3]);
                }
                if (File.Exists("Settings\\Setting_Normal.ini"))
                {
                    var ms = File.ReadAllLines("Settings\\Setting_Normal.ini");
                    GlobalData.VM_MA.CheckFile = ms[0] == "1";
                }
            }
            catch { }
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

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Interop.HwndSource source = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            FolderSelector fs = new FolderSelector()
            {
                DirectoryPath = string.IsNullOrEmpty(GlobalData.VM_MA.SavePath) ? Environment.CurrentDirectory : GlobalData.VM_MA.SavePath
            };
            if (fs.ShowDialog(win) == System.Windows.Forms.DialogResult.OK)
            {
                GlobalData.VM_MA.SavePath = fs.DirectoryPath;
            }
        }

        private async void MouseDoubleClick_Logout(object sender, MouseButtonEventArgs e)
        {
            if (GlobalData.VM_MA.Site == SiteType.Patreon)
            {
                if (await GetCheck(GlobalLanguage.Msg_Logout))
                {
                    if (GlobalData.CheckResult == false)
                    {
                        return;
                    }
                }
                ShowLoading(true);
                await Task.Delay(100);
                _utils = GlobalData.CaptureUtil;
                var _ret = await (_utils as PatreonUtils).InitBrowser();
                if (_ret.Error != ErrorType.NoError)
                {
                    await GetCheck(_ret.Msgs);
                }
                else
                {
                    if ((bool)_ret.Result)
                    {
                        GlobalData.VM_MA.Cookies = string.Empty;
                    }
                    else
                    {
                        await (_utils as PatreonUtils).Logout();
                    }
                }
                ShowLoading(false);
            }
        }

        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            IntPtr _handle;
            public OldWindow(IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members
            IntPtr System.Windows.Forms.IWin32Window.Handle
            {
                get { return _handle; }
            }
            #endregion
        }
    }
}
