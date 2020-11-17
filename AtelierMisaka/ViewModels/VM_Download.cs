using AtelierMisaka.Commands;
using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AtelierMisaka.ViewModels
{
    public class VM_Download : NotifyModel
    {
        private string _exportFile = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location) + "\\Export_Errordownload.txt";
        private bool _canExport = true;

        private bool _isDownloading = false;
        private bool _isChangeThread = false;
        private bool _isChangeProxy = false;
        private bool _isFantia = false;
        private bool _showCheck = false;
        private bool _isQuest = false;
        private string _savePath = string.Empty;
        private string _tempAI = string.Empty;
        private string _tempAN = string.Empty;
        private int _threadCount = 3;
        private double _mLeft = 0d;
        private double _mTop = 0d;
        private SiteType _tempSite = SiteType.Fanbox;

        private List<DownloadItem> _dlClients = new List<DownloadItem>();

        private IList<DownloadItem> _downLoadList = null;
        private IList<DownloadItem> _completedList = null;

        private Dictionary<FantiaItem, HashSet<DownloadItem>> _retryList = null;

        private static readonly object lock_Dl = new object();
        private static readonly object lock_Fantia = new object();

        public int ThreadCount
        {
            get => _threadCount;
            set
            {
                if (_threadCount != value)
                {
                    _threadCount = value;
                    RaisePropertyChanged();
                    if (_isDownloading)
                    {
                        lock (lock_Dl)
                        {
                            if (_dlClients.Count > _threadCount)
                            {
                                for (int i = _dlClients.Count - 1; i >= _threadCount; i++)
                                {
                                    _dlClients[i].Pause();
                                    _dlClients.RemoveAt(i);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < _downLoadList.Count; i++)
                                {
                                    if (_downLoadList[i].DLStatus == DownloadStatus.Waiting)
                                    {
                                        if (!_dlClients.Contains(_downLoadList[i]))
                                        {
                                            _dlClients.Add(_downLoadList[i]);
                                        }
                                        _downLoadList[i].Start();
                                        if (_dlClients.Count >= _threadCount)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool UpdateCul
        {
            set
            {
                RaisePropertyChanged("BtnText");
            }
        }

        public string BtnText
        {
            get => _isDownloading ? GlobalLanguage.Text_AllPause : GlobalLanguage.Text_AllStart;
        }

        public string SavePath
        {
            get => _savePath;
            set
            {
                if (_savePath != value)
                {
                    _savePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string TempAI
        {
            get => _tempAI;
            set
            {
                if (_tempAI != value)
                {
                    _tempAI = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string TempAN
        {
            get => _tempAN;
            set
            {
                if (_tempAN != value)
                {
                    _tempAN = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SiteType TempSite
        {
            get => _tempSite;
            set
            {
                if (_tempSite != value)
                {
                    _tempSite = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowCheck
        {
            get => _showCheck;
            set
            {
                if (_showCheck != value)
                {
                    _showCheck = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                if (_isDownloading != value)
                {
                    _isDownloading = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("BtnText");
                }
            }
        }

        public bool IsChangeThread
        {
            get => _isChangeThread;
            set
            {
                if (_isChangeThread != value)
                {
                    _isChangeThread = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsChangeProxy
        {
            get => _isChangeProxy;
            set
            {
                if (_isChangeProxy != value)
                {
                    _isChangeProxy = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsFantia
        {
            get => _isFantia;
            set
            {
                if (_isFantia != value)
                {
                    _isFantia = value;
                    if (_isFantia)
                    {
                        _retryList = new Dictionary<FantiaItem, HashSet<DownloadItem>>();
                    }
                }
            }
        }

        public double MLeft
        {
            get => _mLeft;
            set
            {
                if (_mLeft != value)
                {
                    _mLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double MTop
        {
            get => _mTop;
            set
            {
                if (_mTop != value)
                {
                    _mTop = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<DownloadItem> DLClients
        {
            get => _dlClients;
            set
            {
                if (_dlClients != value)
                {
                    _dlClients = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IList<DownloadItem> DownLoadItemList
        {
            get => _downLoadList;
            set
            {
                if (_downLoadList != value)
                {
                    _downLoadList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IList<DownloadItem> CompletedList
        {
            get => _completedList;
            set
            {
                if (_completedList != value)
                {
                    _completedList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CommonCommand QuestCommand
        {
            get => new CommonCommand(async() =>
            {
                _isQuest = true;
                await Task.Run(() =>
                {
                    IsDownloading = !_isDownloading;
                    lock (lock_Dl)
                    {
                        if (_isDownloading)
                        {
                            for (int i = 0; i < _downLoadList.Count; i++)
                            {
                                if (_downLoadList[i].DLStatus == DownloadStatus.Waiting)
                                {
                                    if (!_dlClients.Contains(_downLoadList[i]))
                                    {
                                        _dlClients.Add(_downLoadList[i]);
                                    }
                                    _downLoadList[i].Start();
                                    if (_dlClients.Count >= _threadCount)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _dlClients.ForEach(x => x.Pause());
                            _dlClients.Clear();
                        }
                    }
                });
                _isQuest = false;
            },
            () =>
            {
                return !_isQuest && _downLoadList != null && _downLoadList.Count > 0;
            });
        }

        public CommonCommand ExportCommand
        {
            get => new CommonCommand(() =>
            {
                try
                {
                    if (File.Exists(_exportFile))
                    {
                        File.Delete(_exportFile);
                    }
                    for (int i = 0; i < _downLoadList.Count; i++)
                    {
                        if (_downLoadList[i].DLStatus == DownloadStatus.Error)
                        {
                            GlobalMethord.ExportErrorDownload(_downLoadList[i]);
                        }
                    }
                    if (File.Exists(_exportFile))
                    {
                        System.Diagnostics.Process.Start(_exportFile);
                    }
                }
                catch (Exception ex)
                {
                    GlobalMethord.ErrorLog(ex.Message);
                    _canExport = false;
                }
            },
            () => { return _canExport && !_isDownloading; });
        }

        public ParamCommand<DownloadItem> DownloadCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                if (di.DLStatus == DownloadStatus.Downloading)
                {
                    PauseCommand.Execute(di);
                }
                else if (di.DLStatus == DownloadStatus.Paused || di.DLStatus == DownloadStatus.Waiting)
                {
                    StartCommand.Execute(di);
                }
            });
        }

        public ParamCommand<DownloadItem> OptionCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                switch (di.DLStatus)
                {
                    case DownloadStatus.Waiting:
                        ToFirstCommand.Execute(di);
                        break;
                    case DownloadStatus.Cancel:
                        ReStartCommand.Execute(di);
                        break;
                    case DownloadStatus.Common:
                        CancelCommand.Execute(di);
                        break;
                    default:
                        ReStartCommand.Execute(di);
                        break;
                }
            });
        }

        public ParamCommand<DownloadItem> MoveToComLCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                _downLoadList.Remove(di);
                _completedList.Insert(0, di);
            });
        }

        public ParamCommand<object[]> AddCommand
        {
            get => new ParamCommand<object[]>((args) =>
            {
                DownloadItem di = null;
                int index = (int)args[2];
                switch (GlobalData.VM_MA.Site)
                {
                    case SiteType.Fanbox:
                        {
                            BaseItem bi = (BaseItem)args[1];
                            {
                                string sp = $"{_savePath}\\{_tempAN}\\{bi.CreateDate.ToString("yyyyMM\\\\dd_HHmm")}_${bi.Fee}_{bi.Title}";
                                Directory.CreateDirectory(sp);
                                if (!Directory.Exists(sp))
                                {
                                    sp = GlobalMethord.ReplacePath(sp);
                                    Directory.CreateDirectory(sp);
                                }
                                if (index == -1)
                                {
                                    di = new DownloadItem
                                    {
                                        FileName = bi.CoverPicName,
                                        Link = bi.CoverPic,
                                        SavePath = sp,
                                        CTime = bi.CreateDate,
                                        SourceDocu = bi,
                                        AId = _tempAI
                                    };
                                }
                                else
                                {
                                    if ((bool)args[0])
                                    {
                                        di = new DownloadItem
                                        {
                                            FileName = bi.FileNames[index],
                                            Link = bi.ContentUrls[index],
                                            SavePath = sp,
                                            CTime = bi.CreateDate,
                                            SourceDocu = bi,
                                            AId = _tempAI
                                        };
                                    }
                                    else
                                    {
                                        di = new DownloadItem
                                        {
                                            FileName = bi.MediaNames[index],
                                            Link = bi.MediaUrls[index],
                                            SavePath = sp,
                                            CTime = bi.CreateDate,
                                            SourceDocu = bi,
                                            AId = _tempAI
                                        };
                                    }
                                }
                                GlobalData.VM_DL.DownLoadItemList.Add(di);
                            }
                        }
                        break;
                    case SiteType.Fantia:
                        {
                            FantiaItem fi = (FantiaItem)args[1];
                            string sp = $"{_savePath}\\{_tempAN}\\{fi.CreateDate.ToString("yyyyMM\\\\dd_HHmm")}_{fi.Title}";
                            Directory.CreateDirectory(sp);
                            if (!Directory.Exists(sp))
                            {
                                sp = GlobalMethord.ReplacePath(sp);
                                Directory.CreateDirectory(sp);
                            }
                            var nsp = $"{sp}\\{fi.PTitles[index]}";
                            if (!Directory.Exists(nsp))
                            {
                                Directory.CreateDirectory(nsp);
                                if (!Directory.Exists(nsp))
                                {
                                    sp = GlobalMethord.ReplacePath(nsp);
                                    Directory.CreateDirectory(nsp);
                                }
                            }
                            if (index == -1)
                            {
                                di = new DownloadItem
                                {
                                    FileName = fi.CoverPicName,
                                    Link = fi.CoverPic,
                                    SavePath = sp,
                                    SourceDocu = fi,
                                    AId = _tempAI
                                };
                                GlobalData.VM_DL.DownLoadItemList.Add(di);
                            }
                            else
                            {
                                di = new DownloadItem
                                {
                                    FileName = fi.FileNames[index],
                                    Link = fi.ContentUrls[index],
                                    SavePath = nsp,
                                    SourceDocu = fi,
                                    AId = _tempAI
                                };
                            }
                            GlobalData.VM_DL.DownLoadItemList.Add(di);
                        }
                        break;
                    default:
                        {
                            BaseItem bi = (BaseItem)args[1];
                            string sp = $"{_savePath}\\{_tempAN}\\{bi.CreateDate.ToString("yyyyMM\\\\dd_HHmm")}_{bi.Title}";
                            Directory.CreateDirectory(sp);
                            if (!Directory.Exists(sp))
                            {
                                sp = GlobalMethord.ReplacePath(sp);
                                Directory.CreateDirectory(sp);
                            }
                            if (index == -1)
                            {
                                di = new DownloadItem
                                {
                                    FileName = bi.CoverPicName,
                                    Link = bi.CoverPic,
                                    SavePath = sp,
                                    CTime = bi.CreateDate,
                                    SourceDocu = bi,
                                    AId = _tempAI
                                };
                            }
                            else
                            {
                                di = new DownloadItem
                                {
                                    FileName = bi.FileNames[index],
                                    Link = bi.ContentUrls[index],
                                    SavePath = sp,
                                    CTime = bi.CreateDate,
                                    SourceDocu = bi,
                                    AId = _tempAI
                                };
                            }
                            GlobalData.VM_DL.DownLoadItemList.Add(di);
                        }
                        break;
                }
            });
        }

        public ParamCommand<FantiaItem> AddFantiaCommand
        {
            get => new ParamCommand<FantiaItem>((fi) =>
            {
                DownloadItem di = null;
                //foreach (FantiaItem fi in fis)
                {
                    string sp = $"{_savePath}\\{GlobalData.VM_MA.Artist.AName}\\{fi.CreateDate.ToString("yyyyMM\\\\dd_HHmm")}_{fi.Title}";
                    Directory.CreateDirectory(sp);
                    if (!Directory.Exists(sp))
                    {
                        sp = GlobalMethord.ReplacePath(sp);
                        Directory.CreateDirectory(sp);
                    }
                    GlobalData.DLLogs.SetPId(fi.ID);
                    if (!string.IsNullOrEmpty(fi.CoverPic))
                    {
                        if (!GlobalData.DLLogs.HasLog(fi.CoverPic))
                        {
                            di = new DownloadItem
                            {
                                FileName = fi.CoverPicName,
                                Link = fi.CoverPic,
                                SavePath = sp,
                                SourceDocu = fi,
                                AId = _tempAI
                            };
                            GlobalData.SyContext.Send((dd) =>
                            {
                                GlobalData.VM_DL.DownLoadItemList.Add((DownloadItem)dd);
                            }, di);
                        }
                    }
                    for (int i = 0; i < fi.ContentUrls.Count; i++)
                    {
                        if (GlobalMethord.OverPayment(int.Parse(fi.Fees[i])))
                        {
                            continue;
                        }
                        if (!GlobalData.DLLogs.HasLog(fi.ContentUrls[i]))
                        {
                            var nsp = $"{sp}\\{fi.PTitles[i]}";
                            if (!Directory.Exists(nsp))
                            {
                                Directory.CreateDirectory(nsp);
                                if (!Directory.Exists(nsp))
                                {
                                    sp = GlobalMethord.ReplacePath(nsp);
                                    Directory.CreateDirectory(nsp);
                                }
                            }
                            di = new DownloadItem
                            {
                                FileName = fi.FileNames[i],
                                Link = fi.ContentUrls[i],
                                SavePath = nsp,
                                SourceDocu = fi,
                                AId = _tempAI
                            };
                            GlobalData.SyContext.Send((dd) =>
                            {
                                GlobalData.VM_DL.DownLoadItemList.Add((DownloadItem)dd);
                            }, di);
                        }
                    }
                    if (!_isDownloading && QuestCommand.CanExecute(null))
                    {
                        QuestCommand.Execute(null);
                    }
                    if (fi.Comments.Count > 0)
                    {
                        var fp = Path.Combine(sp, "Comment.txt");
                        if (File.Exists(fp))
                        {
                            var cms = File.ReadAllLines(fp);
                            if (cms.Except(fi.Comments).Count() == 0)
                            {
                                return;
                            }
                        }
                        File.WriteAllLines(Path.Combine(sp, "Comment.txt"), fi.Comments);
                    }
                }
            });
        }

        public ParamCommand<DownloadItem> AddRetryCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                if (_isFantia)
                {
                    lock (lock_Fantia)
                    {
                        FantiaItem fi = (FantiaItem)di.SourceDocu;
                        if (_retryList.ContainsKey(fi))
                        {
                            _retryList[fi].Add(di);
                        }
                        else
                        {
                            _retryList.Add(fi, new HashSet<DownloadItem>() { di });
                        }
                    }
                }

            });
        }

        public CommonCommand FantiaRetryCommand
        {
            get => new CommonCommand(() =>
            {
                if (!_isFantia)
                {
                    return;
                }
                DownloadItem di = null;
                foreach (var fi_old in _retryList)
                {
                    foreach (var diold in fi_old.Value)
                    {
                        _downLoadList.Remove(diold);
                    }
                    var fi_new = GlobalData.FantiaRetryUtil.GetUrls(fi_old.Key.ID);
                    for (int i = 0; i < fi_new.ContentUrls.Count; i++)
                    {
                        string sp = $"{_savePath}\\{GlobalData.VM_MA.Artist.AName}\\{fi_new.CreateDate.ToString("yyyyMM\\\\dd_HHmm")}_{fi_new.Title}";
                        Directory.CreateDirectory(sp);
                        if (!Directory.Exists(sp))
                        {
                            sp = GlobalMethord.ReplacePath(sp);
                            Directory.CreateDirectory(sp);
                        }
                        GlobalData.DLLogs.SetPId(fi_new.ID);
                        if (!string.IsNullOrEmpty(fi_new.CoverPic))
                        {
                            if (!GlobalData.DLLogs.HasLog(fi_new.CoverPic))
                            {
                                di = new DownloadItem
                                {
                                    FileName = fi_new.CoverPicName,
                                    Link = fi_new.CoverPic,
                                    SavePath = sp,
                                    SourceDocu = fi_new,
                                    AId = _tempAI
                                };
                                GlobalData.SyContext.Send((dd) =>
                                {
                                    GlobalData.VM_DL.DownLoadItemList.Add((DownloadItem)dd);
                                }, di);
                            }
                        }
                        if (GlobalMethord.OverPayment(int.Parse(fi_new.Fees[i])))
                        {
                            continue;
                        }
                        if (!GlobalData.DLLogs.HasLog(fi_new.ContentUrls[i]))
                        {
                            var nsp = $"{sp}\\{fi_new.PTitles[i]}";
                            if (!Directory.Exists(nsp))
                            {
                                Directory.CreateDirectory(nsp);
                                if (!Directory.Exists(nsp))
                                {
                                    sp = GlobalMethord.ReplacePath(nsp);
                                    Directory.CreateDirectory(nsp);
                                }
                            }
                            di = new DownloadItem
                            {
                                FileName = fi_new.FileNames[i],
                                Link = fi_new.ContentUrls[i],
                                SavePath = nsp,
                                SourceDocu = fi_new,
                                AId = _tempAI
                            };
                            GlobalData.SyContext.Send((dd) =>
                            {
                                GlobalData.VM_DL.DownLoadItemList.Add((DownloadItem)dd);
                            }, di);
                        }
                    }
                    if (!_isDownloading && QuestCommand.CanExecute(null))
                    {
                        QuestCommand.Execute(null);
                    }
                }
            });
        }

        public ParamCommand<DownloadItem> BeginNextCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                if (_isDownloading)
                {
                    lock (lock_Dl)
                    {
                        if (null != di)
                        {
                            _dlClients.RemoveAll(x => x.Equals(di));
                        }
                        for (int i = 0; i < _downLoadList.Count; i++)
                        {
                            if (_dlClients.Count < _threadCount)
                            {
                                if (_downLoadList[i].DLStatus == DownloadStatus.Waiting)
                                {
                                    if (!_dlClients.Contains(_downLoadList[i]))
                                    {
                                        _dlClients.Add(_downLoadList[i]);
                                    }
                                    _downLoadList[i].Start();
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        if (_dlClients.Count == 0)
                        {
                            IsDownloading = false;
                            if (_downLoadList.Count == 0)
                            {
                                if (!_isFantia)
                                {
                                    GlobalData.VM_MA.Date = GlobalData.StartTime.ToString("yyyy/MM/dd HH:mm:ss");
                                    GlobalData.LastDateDic.Update(GlobalData.VM_MA.LastDate);
                                }
                                else
                                {
                                    if (_retryList.Count != 0 && GlobalData.VM_MA.IsStarted)
                                    {
                                        FantiaRetryCommand.Execute(null);
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        public ParamCommand<DownloadItem> OpenFileCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                System.Diagnostics.Process.Start($"{di.SavePath}\\{di.FileName}");
            });
        }

        public ParamCommand<DownloadItem> OpenFolderCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                GlobalMethord.ExplorerFile($"{di.SavePath}\\{di.FileName}");
            });
        }

        public ParamCommand<DownloadItem> OpenDocumentCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                GlobalCommand.ShowDocumentCommand.Execute(di.SourceDocu);
                System.Windows.Application.Current.MainWindow.Activate();
            });
        }

        public ParamCommand<DownloadItem> ToFirstCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                _downLoadList.Remove(di);
                _downLoadList.Insert(0, di);
            });
        }

        public ParamCommand<DownloadItem> StartCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                if (_dlClients.Count < _threadCount)
                {
                    lock (lock_Dl)
                    {
                        if (_dlClients.Count < _threadCount)
                        {
                            if (!_dlClients.Contains(di))
                            {
                                _dlClients.Add(di);
                            }
                            di.ReTryCount = 0;
                            di.Start();
                            if (_dlClients.Count == _threadCount || _dlClients.Count == _downLoadList.Count)
                            {
                                IsDownloading = true;
                            }
                        }
                    }
                }
                else
                {
                    di.DLStatus = DownloadStatus.Waiting;
                }
            });
        }

        public ParamCommand<DownloadItem> PauseCommand
        {
            get => new ParamCommand<DownloadItem>(async (di) =>
            {
                await di.Pause();
                BeginNextCommand.Execute(di);
            });
        }

        public ParamCommand<DownloadItem> ReStartCommand
        {
            get => new ParamCommand<DownloadItem>(async (di) =>
            {
                if (di.DLStatus == DownloadStatus.Downloading)
                {
                    if (await di.Pause())
                    {
                        di.Start();
                    }
                }
                else if (di.DLStatus != DownloadStatus.WriteFile)
                {
                    di.DLStatus = DownloadStatus.Waiting;
                    _downLoadList.Remove(di);
                    _downLoadList.Insert(0, di);
                }
            });
        }

        public ParamCommand<DownloadItem> CancelCommand
        {
            get => new ParamCommand<DownloadItem>(async (di) =>
            {
                if (di.DLStatus == DownloadStatus.Cancel)
                    return;
                
                if (di.DLStatus == DownloadStatus.Downloading)
                {
                    await di.Cancel();
                    BeginNextCommand.Execute(di);
                }
                else
                {
                    di.DLStatus = DownloadStatus.Cancel;
                }
                _downLoadList.Remove(di);
                _downLoadList.Add(di);
            });
        }

    }
}
