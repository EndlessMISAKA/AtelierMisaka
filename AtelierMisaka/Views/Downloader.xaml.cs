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
        //string _tempAI = string.Empty;

        public Downloader(IList<BaseItem> bis, string savepath)
        {
            InitializeComponent();
            _baseItems = bis.Reverse().ToList();
            GlobalData.VM_DL = VM_DD;
            VM_DD.TempAI = GlobalData.VM_MA.Artist.Id;
            VM_DD.SavePath = savepath;
            GlobalData.Dbl.InsertDate(VM_DD.TempAI, GlobalData.VM_MA.LastDate);
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
        }

        public async void LoadData()
        {
            VM_DD.CompletedList = new ObservableCollection<DownloadItem>();
            var _downLoadItemList = new ObservableCollection<DownloadItem>();
            await Task.Run(() =>
            {
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
                                AId = VM_DD.TempAI
                            };
                            di.CheckTempFile();
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
                                AId = VM_DD.TempAI
                            };
                            di.CheckTempFile();
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
                                AId = VM_DD.TempAI
                            };
                            di.CheckTempFile();
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
