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
        private string _tempSP = string.Empty;
        private bool _tempUP = false;
        private bool _tempUD = false;

        private bool _second = false;

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
            if (!GlobalData.VM_MA.IsStarted)
            {
                ShowLoading(true);
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
                    _tempDate = GlobalData.VM_MA.Date;
                    _tempSP = GlobalData.VM_MA.SavePath;
                    _tempUP = GlobalData.VM_MA.UseProxy;
                    _tempUD = GlobalData.VM_MA.UseDate;

                    ShowLoading(false);
                    GlobalData.VM_MA.IsStarted = true;
                }
            }
            else
            {
                if (_tempSite != GlobalData.VM_MA.Site || _tempArt.PostUrl != GlobalData.VM_MA.Artist.PostUrl)
                {
                    ShowLoading(true);
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
                        _tempDate = GlobalData.VM_MA.Date;
                        _tempSP = GlobalData.VM_MA.SavePath;
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
                            bi.Skip = GlobalMethord.OverTime(bi.UpdateDate);
                        }
                    });
                    GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
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
                GlobalCommand.BackCommand.Execute(BackType.Pop);
            }
            _second = false;
        }

        private async Task<bool> Begin()
        {
            if (string.IsNullOrEmpty(GlobalData.VM_MA.Cookies))
            {
                await GetCheck(GlobalLanguage.Msg_CheckCk);
                ShowLoading(false);
                return false;
            }

            if (string.IsNullOrEmpty(GlobalData.VM_MA.SavePath))
            {
                await GetCheck(GlobalLanguage.Msg_CheckSP);
                ShowLoading(false);
                return false;
            }
            Directory.CreateDirectory(GlobalData.VM_MA.SavePath);
            if (!Directory.Exists(GlobalData.VM_MA.SavePath))
            {
                await GetCheck(GlobalLanguage.Msg_CreateSP);
                ShowLoading(false);
                return false;
            }

            if (null != GlobalData.DownLP)
            {
                GlobalData.VM_DL.DownLoadItemList.Clear();
                GlobalData.DownLP.Close();
            }
            ResultMessage _ret = null;
            _utils = GlobalData.CaptureUtil;
            if (_utils is PatreonUtils)
            {
                _ret = await (_utils as PatreonUtils).InitBrowser();
                if (_ret.Error != ErrorType.NoError)
                {
                    await GetCheck(_ret.Msgs);
                    ShowLoading(false);
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
                        ShowLoading(false);
                        return false;
                    }
                }
            }
            if (!GlobalData.VM_MA.HasSelected)
            {
                ArtistInfo ai = null;
                _ret = await _utils.GetArtistInfo(GlobalData.VM_MA.Artist.PostUrl);
                if (_ret.Error != ErrorType.NoError)
                {
                    await GetCheck(_ret.Msgs);
                    ShowLoading(false);
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
                SetLastDate(ai.Id);
            }
            Task.Run(() => SaveSetting());
            await Task.Run(() => GlobalData.DLLogs.LoadData(GlobalData.VM_MA.Artist.Id, GlobalData.VM_MA.Site));
            _ret = await _utils.GetPostIDs(GlobalData.VM_MA.Artist.Cid);
            if (_ret.Error != ErrorType.NoError)
            {
                await GetCheck(_ret.Msgs);
                ShowLoading(false);
                return false;
            }
            _tempBis = (List<BaseItem>)_ret.Result;
            if (_tempBis.Count == 0)
            {
                await GetCheck(GlobalLanguage.Msg_NoPosts);
                ShowLoading(false);
                return false;
            }
            GlobalData.VM_MA.ItemList = _tempBis.Where(x => !x.Skip).ToList();
            GlobalData.DownLP = new Downloader(_tempBis);
            GlobalData.DownLP.Show();
            GlobalData.DownLP.LoadData();
            GlobalCommand.BackCommand.Execute(BackType.Pop);
            return true;
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

        private async Task<bool> GetCheck(params string[] msgs)
        {
            string mss = msgs[0];
            for (int i = 1; i < msgs.Length; i++)
            {
                mss += Environment.NewLine;
                mss += msgs[i];
            }
            GlobalData.VM_MA.Messages = mss;
            
            GlobalData.VM_MA.MLeft = (ActualWidth - 400) / 2;
            GlobalData.VM_MA.MTop = (ActualHeight - 270) / 2;
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
            }
            catch { }
        }

        private void Lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ArtistInfo ai = (ArtistInfo)e.AddedItems[0];
                SetLastDate(ai.Id);
            }
        }

        private void SetLastDate(string id)
        {
            if (GlobalData.LastDateDic.TryGetValue(GlobalData.VM_MA.Site, id, out DateTime dt))
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
    }
}
