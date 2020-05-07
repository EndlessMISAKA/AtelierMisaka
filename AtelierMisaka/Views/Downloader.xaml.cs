using AtelierMisaka.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AtelierMisaka.Views
{
    /// <summary>
    /// Downloader.xaml 的交互逻辑
    /// </summary>
    public partial class Downloader : Window
    {
        IList<BaseItem> _baseItems = null;
        bool _mouseD = false;
        Point _mouM = new Point(0, 0);

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
                if (GlobalData.VM_MA.Site == SiteType.Fantia)
                {
                    foreach (FantiaItem fi in _baseItems)
                    {
                        string sp = $"{VM_DD.SavePath}\\{GlobalData.VM_MA.Artist.AName}\\{fi.CreateDate.ToString("yyyyMMdd_HHmm")}_{(fi.Title)}";
                        Directory.CreateDirectory(sp);
                        if (!Directory.Exists(sp))
                        {
                            sp = GlobalData.ReplacePath(sp);
                            Directory.CreateDirectory(sp);
                        }
                        haslog = GlobalData.DownloadLogs.TryGetValue(fi.ID, out temdl);
                        if (!string.IsNullOrEmpty(fi.CoverPic))
                        {
                            if (!haslog || !temdl.Exists(x => x.Link.Contains(fi.CoverPic)))
                            {
                                di = new DownloadItem
                                {
                                    FileName = $"Cover.{fi.CoverPic.Split('.').Last()}",
                                    Link = fi.CoverPic,
                                    SavePath = sp,
                                    SourceDocu = fi,
                                    AId = VM_DD.TempAI
                                };
                                di.CheckTempFile();
                                _downLoadItemList.Add(di);
                            }
                        }
                        for (int i = 0; i < fi.ContentUrls.Count; i++)
                        {
                            if (GlobalData.OverPayment(int.Parse(fi.Fees[i])))
                            {
                                continue;
                            }
                            int ind = fi.ContentUrls[i].IndexOf("?Key");
                            var turl = (ind == -1) ? fi.ContentUrls[i] : fi.ContentUrls[i].Substring(0, ind);
                            if (haslog && temdl.Exists(x => x.Link.Contains(turl)))
                            {
                                continue;
                            }
                            else
                            {
                                var nsp = $"{sp}\\{fi.PTitles[i]}";
                                if (!Directory.Exists(nsp))
                                {
                                    Directory.CreateDirectory(nsp);
                                    if (!Directory.Exists(nsp))
                                    {
                                        sp = GlobalData.ReplacePath(nsp);
                                        Directory.CreateDirectory(nsp);
                                    }
                                }
                                di = new DownloadItem
                                {
                                    FileName = fi.FileNames[i],
                                    Link = fi.ContentUrls[i],
                                    SavePath = nsp,
                                    SourceDocu = fi,
                                    AId = VM_DD.TempAI
                                };
                                di.CheckTempFile();
                                _downLoadItemList.Add(di);
                            }
                        }
                        if (fi.Comments.Count > 0)
                        {
                            File.WriteAllLines(Path.Combine(sp, "Comment.txt"), fi.Comments);
                        }
                    }
                }
                else
                {
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
                }
            });
            VM_DD.DownLoadItemList = _downLoadItemList;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (VM_DD.DownLoadItemList.Count > 0)
            {
                VM_DD.MLeft = (ActualWidth - 400) / 2;
                VM_DD.MTop = (ActualHeight - 300) / 2;
                VM_DD.ShowCheck = true;
                e.Cancel = true;
            }
        }

        #region TitleButton

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

        #endregion

        #region Check

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            VM_DD.ShowCheck = false;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            VM_DD.ShowCheck = false;
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
                VM_DD.MLeft += (mm.X - _mouM.X);
                VM_DD.MTop += (mm.Y - _mouM.Y);
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

        #endregion
    }
}
