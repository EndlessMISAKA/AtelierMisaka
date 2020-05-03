using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace AtelierMisaka.Views
{
    /// <summary>
    /// Downloader.xaml 的交互逻辑
    /// </summary>
    public partial class Downloader : Window
    {
        IList<BaseItem> _baseItems = null;
        int _changedCount = 0;
        string _tempAI = string.Empty;
        //DateTime _tempDt = DateTime.Parse("2010/01/01");
        //bool _tflag = true;
        
        static readonly object lock_Wc = new object();
        static readonly object lock_Ft = new object();

        public Downloader(IList<BaseItem> bis, string savepath)
        {
            InitializeComponent();
            _baseItems = bis.Reverse().ToList();
            _tempAI = GlobalData.VM_MA.Artist.Id;
            GlobalData.VM_DL = VM_DD;
            VM_DD.SavePath = savepath;
            //Directory.CreateDirectory("Temp");
            var ft = GlobalData.Dbl.GetLastDate(_tempAI, out DateTime dt);
            if (ft == true)
            {
                //_tempDt = dt;
                //_tempDt = DateTime.Parse(File.ReadAllText($"Temp\\{_tempAI}"));
                if (GlobalData.VM_MA.LastDate > dt)
                {
                    GlobalData.Dbl.UpdateDate(_tempAI, GlobalData.VM_MA.LastDate);
                }
            }
            else
            {
                if (!GlobalData.Dbl.InsertDate(_tempAI, GlobalData.VM_MA.LastDate))
                {
                    GlobalData.Dbl.UpdateDate(_tempAI, GlobalData.VM_MA.LastDate);
                }
                //if (ft == false)
                //{
                //    _tflag = false;
                //}
                //else
                //{
                //    if (!GlobalData.VM_MA.LastDate.Equals(DateTime.MinValue))
                //    {
                //    }
                //    else
                //    {
                //        _tflag = GlobalData.Dbl.InsertDate(_tempAI, _tempDt);
                //    }
                //    //if (!GlobalData.Dbl.InsertDate(_tempAI, _tempDt))
                //    //    _tflag = false;
                //}
            }
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
            //VM_DD.IsLoading = true;
        }

        public async void LoadData()
        {
            VM_DD.CompletedList = new ObservableCollection<DownloadItem>();
            var _downLoadItemList = new ObservableCollection<DownloadItem>();
            await Task.Run(() =>
            {
                //VM_DD.PostCount = _baseItems.Count;
                DownloadItem di = null;
                List<DLSP> temdl = null;
                bool haslog = false;
                foreach (var bi in _baseItems)
                {
                    if (bi.Skip)
                    {
                        continue;
                    }
                    string sp = $"{VM_DD.SavePath}\\{GlobalData.VM_MA.Artist.AName}\\{bi.CreateDate.ToString("yyyyMMdd_HHmm")}_${bi.Fee}_{(bi.Title)}";
                    Directory.CreateDirectory(sp);
                    if (!Directory.Exists(sp))
                    {
                        sp = GlobalData.ReplacePath(sp);
                        Directory.CreateDirectory(sp);
                    }
                    haslog = GlobalData.DownloadLogs.TryGetValue(bi.ID, out temdl);
                    if (!string.IsNullOrEmpty(bi.CoverPic))
                    {
                        if (!haslog || !temdl.Exists(x => x.Link.Contains(bi.CoverPic)))
                        {
                            di = new DownloadItem
                            {
                                FileName = $"Cover.{bi.CoverPic.Split('.').Last()}",
                                Link = bi.CoverPic,
                                SavePath = sp,
                                CTime = bi.CreateDate,
                                SourceDocu = bi,
                                AId = _tempAI
                            };
                            _downLoadItemList.Add(di);
                        }
                    }
                    for (int i = 0; i < bi.ContentUrls.Count; i++)
                    {
                        if (haslog && temdl.Exists(x => x.Link.Contains(bi.ContentUrls[i])))
                        {
                            continue;
                        }
                        else
                        {
                            di = new DownloadItem
                            {
                                FileName = bi.FileNames[i],
                                Link = bi.ContentUrls[i],
                                SavePath = sp,
                                CTime = bi.CreateDate,
                                SourceDocu = bi,
                                AId = _tempAI
                            };
                            _downLoadItemList.Add(di);
                        }
                    }
                    for (int i = 0; i < bi.MediaUrls.Count; i++)
                    {
                        if (haslog && temdl.Exists(x => x.Link.Contains(bi.MediaUrls[i])))
                        {
                            continue;
                        }
                        else
                        {
                            di = new DownloadItem
                            {
                                FileName = bi.MediaNames[i],
                                Link = bi.MediaUrls[i],
                                SavePath = sp,
                                CTime = bi.CreateDate,
                                SourceDocu = bi,
                                AId = _tempAI
                            };
                            _downLoadItemList.Add(di);
                        }
                    }
                    if (bi.Comments.Count > 0)
                    {
                        if (haslog)
                        {
                            continue;
                        }
                        File.WriteAllLines(Path.Combine(sp, "Comment.txt"), bi.Comments);
                    }
                }
            });
            VM_DD.DownLoadItemList = _downLoadItemList;
        }

        private void ThreadCountChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count > 0)
            //{
            //    TextBlock tb = (TextBlock)e.AddedItems[0];
            //    if (int.TryParse(tb.Text, out int count))
            //    {
            //        if (VM_DD.IsDownloading)
            //        {
            //            if (count > VM_DD.WClients.Count)
            //            {
            //                count -= VM_DD.WClients.Count;
            //                for (int i = 0; i < count; i++)
            //                {
            //                    WebClient wc = new WebClient();
            //                    wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
            //                    wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            //                    wc.Proxy = GlobalData.VM_MA.MyProxy;
            //                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
            //                    wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
            //                    VM_DD.WClients.Add(wc);
            //                    lock (lock_Wc)
            //                    {
            //                        for (int index = 0; index < VM_DD.DownLoadItemList.Count; index++)
            //                        {
            //                            if (VM_DD.DownLoadItemList[index].DLStatus == DownloadStatus.Waiting)
            //                            {
            //                                VM_DD.DownLoadItemList[index].DClient = wc;
            //                                break;
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                VM_DD.IsChangeThread = true;
            //                Interlocked.Exchange(ref _changedCount, VM_DD.WClients.Count - count);
            //                for (int i = VM_DD.WClients.Count - 1; i >= count; i--)
            //                {
            //                    VM_DD.WClients[i].CancelAsync();
            //                    VM_DD.WClients.RemoveAt(i);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            if (VM_DD.WClients != null)
            //            {
            //                VM_DD.WClients.ForEach(x => x.Dispose());
            //                VM_DD.WClients.Clear();
            //            }
            //            VM_DD.WClients = new List<WebClient>();
            //            for (int i = 0; i < count; i++)
            //            {
            //                WebClient wc = new WebClient();
            //                wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            //                wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
            //                wc.Proxy = GlobalData.VM_MA.MyProxy;
            //                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
            //                wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
            //                VM_DD.WClients.Add(wc);
            //            }
            //        }
            //    }
            //}
        }

        //private async void Wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        //{
        //    DownloadItem di = (DownloadItem)e.UserState;
        //    if (e.Cancelled)
        //    {
        //        if (di.IsReStart)
        //        {
        //            GC.Collect(9);
        //            di.DClient = (WebClient)sender;
        //            return;
        //        }
        //        else if (VM_DD.IsChangeThread)
        //        {
        //            di.DLStatus = DownloadStatus.Waiting;
        //            di.DClient = null;
        //            ((WebClient)sender).Dispose();
        //            Interlocked.Decrement(ref _changedCount);
        //            if (_changedCount == 0)
        //            {
        //                VM_DD.IsChangeThread = false;
        //            }
        //            GC.Collect(9);
        //            return;
        //        }
        //        else if (!VM_DD.IsDownloading)
        //        {
        //            GC.Collect(9);
        //            di.DLStatus = DownloadStatus.Waiting;
        //            di.DClient = null;
        //            return;
        //        }
        //        else
        //        {
        //            di.DLStatus = DownloadStatus.Cancel;
        //        }
        //    }
        //    else if (e.Error != null)
        //    {
        //        di.ReTryCount++;
        //        if (di.ReTryCount < 10)
        //        {
        //            GC.Collect(9);
        //            di.DClient = (WebClient)sender;
        //            return;
        //        }
        //        else
        //        {
        //            di.DLStatus = DownloadStatus.Error;
        //        }
        //    }
        //    else
        //    {
        //        await Task.Run(() =>
        //        {
        //            var fn = Path.Combine(di.SavePath, di.FileName);
        //            if (File.Exists(fn))
        //            {
        //                string tn = DateTime.Now.ToString("yyyyMMdd_HHmm");
        //                fn += $"_{tn}";
        //                di.FileName += $"_{tn}";
        //            }
        //            File.WriteAllBytes(fn, e.Result);
        //            lock (lock_Ft)
        //            {
        //                //if (di.CTime > _tempDt)
        //                //{
        //                //    //File.WriteAllText($"Temp\\{_tempAI}", di.CTime.ToString("yyyy/MM/dd HH:mm"));
        //                //    //GlobalData.Dbl.UpdateDate(_tempAI, di.CTime);
        //                //    _tempDt = di.CTime;
        //                //}
        //                GlobalData.Dbl.InsertLog(_tempAI, di.SourceDocu.ID, fn, di.Link);
        //            }
        //        });
        //        di.DLStatus = DownloadStatus.Completed;
        //    }
        //    GC.Collect(9);
        //    di.DClient = null;
        //    bool flag = false;
        //    lock (lock_Wc)
        //    {
        //        if (di.DLStatus == DownloadStatus.Completed)
        //        {
        //            Dispatcher.Invoke(() =>
        //            {
        //                VM_DD.DownLoadItemList.Remove(di);
        //                VM_DD.CompletedList.Insert(0, di);
        //            });
        //        }
        //        for (int i = 0; i < VM_DD.DownLoadItemList.Count; i++)
        //        {
        //            if (VM_DD.DownLoadItemList[i].DLStatus == DownloadStatus.Waiting)
        //            {
        //                VM_DD.DownLoadItemList[i].DClient = (WebClient)sender;
        //                return;
        //            }
        //            else if (VM_DD.DownLoadItemList[i].DLStatus == DownloadStatus.Downloading)
        //            {
        //                flag = true;
        //            }
        //        }
        //    }
        //    if (flag)
        //    {
        //        return;
        //    }
        //    //await Task.Run(() =>
        //    //{
        //    //    if (_tflag)
        //    //        GlobalData.Dbl.UpdateDate(_tempAI, _tempDt);
        //    //});
        //    VM_DD.IsDownloading = false;
        //    VM_DD.BtnText = "全部开始";
        //}

        //private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        //{
        //    DownloadItem di = (DownloadItem)e.UserState;
        //    di.Percent = e.ProgressPercentage;
        //    di.TotalRC = e.BytesReceived;
        //    di.ContentLength = e.TotalBytesToReceive;
        //    //if (di.ContentLength == 0)
        //    //{
        //    //    if (long.TryParse(di.DClient.ResponseHeaders[HttpResponseHeader.ContentLength], out long ll))
        //    //    {
        //    //        di.ContentLength = ll;
        //    //    }
        //    //    else
        //    //    {
        //    //        di.ContentLength = -1;
        //    //    }
        //    //}
        //}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (VM_DD.DownLoadItemList.Count > 0)
            {
                VM_DD.ShowCheck = true;
                e.Cancel = true;
            }
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            VM_DD.ShowCheck = false;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            VM_DD.ShowCheck = false;
        }

        private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode != ResizeMode.NoResize;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }
    }
}
