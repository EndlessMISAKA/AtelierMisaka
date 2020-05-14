using AtelierMisaka.Commands;
using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AtelierMisaka.ViewModels
{
    public class VM_Download : NotifyModel
    {
        private bool _isDownloading = false;
        private bool _isChangeThread = false;
        private bool _isChangeProxy = false;
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

        private static readonly object lock_Dl = new object();

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

        public string BtnText
        {
            get => _isDownloading ? "全部暂停" : "全部开始";
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
                                string sp = $"{_savePath}\\{_tempAN}\\{bi.CreateDate.ToString("yyyyMMdd_HHmm")}_${bi.Fee}_{bi.Title}";
                                Directory.CreateDirectory(sp);
                                if (!Directory.Exists(sp))
                                {
                                    sp = GlobalData.ReplacePath(sp);
                                    Directory.CreateDirectory(sp);
                                }
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
                                    GlobalData.VM_DL.DownLoadItemList.Add(di);
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
                                    GlobalData.VM_DL.DownLoadItemList.Add(di);
                                }
                            }
                        }
                        break;
                    case SiteType.Fantia:
                        {
                            FantiaItem fi = (FantiaItem)args[1];
                            string sp = $"{_savePath}\\{_tempAN}\\{fi.CreateDate.ToString("yyyyMMdd_HHmm")}_{fi.Title}";
                            Directory.CreateDirectory(sp);
                            if (!Directory.Exists(sp))
                            {
                                sp = GlobalData.ReplacePath(sp);
                                Directory.CreateDirectory(sp);
                            }
                            var nsp = $"{sp}\\{fi.PTitles[index]}";
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
                                FileName = fi.FileNames[index],
                                Link = fi.ContentUrls[index],
                                SavePath = nsp,
                                SourceDocu = fi,
                                AId = _tempAI
                            };
                            di.CheckTempFile();
                            GlobalData.VM_DL.DownLoadItemList.Add(di);
                        }
                        break;
                    default:
                        {
                            BaseItem bi = (BaseItem)args[1];
                            string sp = $"{_savePath}\\{_tempAN}\\{bi.CreateDate.ToString("yyyyMMdd_HHmm")}_{bi.Title}";
                            Directory.CreateDirectory(sp);
                            if (!Directory.Exists(sp))
                            {
                                sp = GlobalData.ReplacePath(sp);
                                Directory.CreateDirectory(sp);
                            }
                            di = new DownloadItem
                            {
                                FileName = bi.FileNames[index],
                                Link = bi.ContentUrls[index],
                                SavePath = sp,
                                CTime = bi.CreateDate,
                                SourceDocu = bi,
                                AId = _tempAI
                            };
                            GlobalData.VM_DL.DownLoadItemList.Add(di);
                        }
                        break;
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
                            _dlClients.Remove(di);
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
                                GlobalData.VM_MA.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                                GlobalData.LastDateDic.Update(GlobalData.VM_MA.LastDate);
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
                GlobalData.ExplorerFile($"{di.SavePath}\\{di.FileName}");
            });
        }

        public ParamCommand<DownloadItem> OpenDocumentCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                GlobalData.ShowDocumentCommand.Execute(di.SourceDocu);
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
