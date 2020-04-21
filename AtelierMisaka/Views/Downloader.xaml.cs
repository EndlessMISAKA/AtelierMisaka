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
        DateTime _tempDt = DateTime.MinValue;
        
        static readonly object lock_Wc = new object();
        static readonly object lock_Ft = new object();

        public Downloader(IList<BaseItem> bis, string savepath)
        {
            InitializeComponent();
            _baseItems = bis.Reverse().ToList();
            _tempAI = GlobalData.VM_MA.Artist.Id;
            GlobalData.VM_DL = VM_DD;
            VM_DD.SavePath = savepath;
            Directory.CreateDirectory("Temp");
            if (File.Exists($"Temp\\{_tempAI}"))
            {
                _tempDt = DateTime.Parse(File.ReadAllText($"Temp\\{_tempAI}"));
            }
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
            VM_DD.IsLoading = true;
        }

        public async void LoadData()
        {
            await Task.Run(() =>
            {
                VM_DD.DownLoadItemList = new ObservableCollection<DownloadItem>();
                VM_DD.CompletedList = new ObservableCollection<DownloadItem>();
                VM_DD.PostCount = _baseItems.Count;
                DownloadItem di = null;
                foreach (var bi in _baseItems)
                {
                    if (bi.Skip)
                    {
                        continue;
                    }
                    string sp = $"{VM_DD.SavePath}\\{GlobalData.VM_MA.Artist.AName}\\{bi.CreateDate.ToString("yyyyMMdd_HHmm")}_${bi.Fee}_{(bi.Title)}";
                    if (!string.IsNullOrEmpty(bi.CoverPic))
                    {
                        di = new DownloadItem
                        {
                            FileName = $"Cover.{bi.CoverPic.Split('.').Last()}",
                            Link = bi.CoverPic,
                            SavePath = sp,
                            CTime = bi.CreateDate,
                            SourceDocu = bi
                        };
                        VM_DD.DownLoadItemList.Add(di);
                    }
                    for (int i = 0; i < bi.ContentUrls.Count; i++)
                    {
                        di = new DownloadItem
                        {
                            FileName = bi.FileNames[i],
                            Link = bi.ContentUrls[i],
                            SavePath = sp,
                            CTime = bi.CreateDate,
                            SourceDocu = bi
                        };
                        VM_DD.DownLoadItemList.Add(di);
                    }
                    for (int i = 0; i < bi.MediaUrls.Count; i++)
                    {
                        di = new DownloadItem
                        {
                            FileName = bi.MediaNames[i],
                            Link = bi.MediaUrls[i],
                            SavePath = sp,
                            CTime = bi.CreateDate,
                            SourceDocu = bi
                        };
                        VM_DD.DownLoadItemList.Add(di);
                    }
                    Directory.CreateDirectory(sp);
                    if (bi.Comments.Count > 0)
                    {
                        File.WriteAllLines(Path.Combine(sp, "Comment.txt"), bi.Comments);
                    }
                    VM_DD.LoadCount++;
                }
            });
            VM_DD.IsLoading = false;
        }

        private void ThreadCountChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                TextBlock tb = (TextBlock)e.AddedItems[0];
                if (int.TryParse(tb.Text, out int count))
                {
                    if (VM_DD.IsDownloading)
                    {
                        if (count > VM_DD.WClients.Count)
                        {
                            count -= VM_DD.WClients.Count;
                            for (int i = 0; i < count; i++)
                            {
                                WebClient wc = new WebClient();
                                wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                                wc.Proxy = GlobalData.VM_MA.MyProxy;
                                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                                wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
                                VM_DD.WClients.Add(wc);
                                lock (lock_Wc)
                                {
                                    for (int index = 0; index < VM_DD.DownLoadItemList.Count; index++)
                                    {
                                        if (VM_DD.DownLoadItemList[index].DLStatus == DownloadStatus.Waiting)
                                        {
                                            VM_DD.DownLoadItemList[index].DClient = wc;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            VM_DD.IsChangeThread = true;
                            Interlocked.Exchange(ref _changedCount, VM_DD.WClients.Count - count);
                            for (int i = VM_DD.WClients.Count - 1; i >= count; i--)
                            {
                                VM_DD.WClients[i].CancelAsync();
                                VM_DD.WClients.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        if (VM_DD.WClients != null)
                        {
                            VM_DD.WClients.ForEach(x => x.Dispose());
                            VM_DD.WClients.Clear();
                        }
                        VM_DD.WClients = new List<WebClient>();
                        for (int i = 0; i < count; i++)
                        {
                            WebClient wc = new WebClient();
                            wc.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                            wc.Proxy = GlobalData.VM_MA.MyProxy;
                            wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                            wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
                            VM_DD.WClients.Add(wc);
                        }
                    }
                }
            }
        }

        private async void Wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            DownloadItem di = (DownloadItem)e.UserState;
            if (e.Cancelled)
            {
                if (di.IsReStart)
                {
                    di.DClient = (WebClient)sender;
                    return;
                }
                else if (VM_DD.IsChangeThread)
                {
                    di.DLStatus = DownloadStatus.Waiting;
                    di.DClient = null;
                    ((WebClient)sender).Dispose();
                    Interlocked.Decrement(ref _changedCount);
                    if (_changedCount == 0)
                    {
                        VM_DD.IsChangeThread = false;
                    }
                    return;
                }
                else if (!VM_DD.IsDownloading)
                {
                    di.DLStatus = DownloadStatus.Waiting;
                    di.DClient = null;
                    return;
                }
                else
                {
                    di.DLStatus = DownloadStatus.Cancel;
                }
            }
            else if (e.Error != null)
            {
                di.ReTryCount++;
                if (di.ReTryCount < 10)
                {
                    di.DClient = (WebClient)sender;
                    return;
                }
                else
                {
                    di.DLStatus = DownloadStatus.Error;
                }
            }
            else
            {
                await Task.Run(() =>
                {
                    File.WriteAllBytes(Path.Combine(di.SavePath, di.FileName), e.Result);
                    lock (lock_Ft)
                    {
                        if (di.CTime > _tempDt)
                        {
                            File.WriteAllText($"Temp\\{_tempAI}", di.CTime.ToString("yyyy/MM/dd HH:mm"));
                            _tempDt = di.CTime;
                        }
                    }
                });
                di.DLStatus = DownloadStatus.Completed;
            }
            di.DClient = null;
            bool flag = false;
            lock (lock_Wc)
            {
                if (di.DLStatus == DownloadStatus.Completed)
                {
                    Dispatcher.Invoke(() =>
                    {
                        VM_DD.DownLoadItemList.Remove(di);
                        VM_DD.CompletedList.Insert(0, di);
                    });
                }
                for (int i = 0; i < VM_DD.DownLoadItemList.Count; i++)
                {
                    if (VM_DD.DownLoadItemList[i].DLStatus == DownloadStatus.Waiting)
                    {
                        VM_DD.DownLoadItemList[i].DClient = (WebClient)sender;
                        return;
                    }
                    else if (VM_DD.DownLoadItemList[i].DLStatus == DownloadStatus.Downloading)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                return;
            }
            VM_DD.IsDownloading = false;
            VM_DD.BtnText = "全部开始";
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadItem di = (DownloadItem)e.UserState;
            di.Percent = e.ProgressPercentage;
            //di.ContentLength = e.TotalBytesToReceive;
            di.TotalRC = e.BytesReceived;
            if (di.ContentLength == 0)
            {
                if (long.TryParse(di.DClient.ResponseHeaders[HttpResponseHeader.ContentLength], out long ll))
                {
                    di.ContentLength = ll;
                }
            }
        }

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
            if (VM_DD.IsDownloading)
            {
                VM_DD.IsDownloading = false;
                VM_DD.WClients.ForEach(x => x.CancelAsync());
            }
            VM_DD.DownLoadItemList.Clear();
            this.Close();
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
