using AtelierMisaka.Commands;
using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
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
        private string _btnText = "全部开始";
        private string _savePath = string.Empty;
        private string _tempAI = string.Empty;
        private int _threadCount = 3;
        private double _mLeft = 0d;
        private double _mTop = 0d;

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
                                    _dlClients.Add(_downLoadList[i]);
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

        public string BtnText
        {
            get => _btnText;
            set
            {
                if (_btnText != value)
                {
                    _btnText = value;
                    RaisePropertyChanged();
                }
            }
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
                    _isDownloading = !_isDownloading;
                    if (_isDownloading)
                    {
                        BtnText = "全部暂停";
                        for (int i = 0; i < _downLoadList.Count; i++)
                        {
                            if (_downLoadList[i].DLStatus == DownloadStatus.Waiting)
                            {
                                _dlClients.Add(_downLoadList[i]);
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
                        BtnText = "全部开始";
                        _dlClients.ForEach(x => x.Pause());
                        _dlClients.Clear();
                    }
                });
                _isQuest = false;
            },
            () =>
            {
                return !_isQuest && _downLoadList != null && _downLoadList.Count > 0;
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
                                    _dlClients.Add(_downLoadList[i]);
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
                            BtnText = "全部开始";
                            GlobalData.VM_MA.LastDate = DateTime.Now;
                            GlobalData.Dbl.UpdateDate(_tempAI, GlobalData.VM_MA.LastDate);
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
                    _dlClients.Add(di);
                    di.Start();
                    //if (_dlClients.Count == _threadCount)
                    //{
                    //    BtnText = "全部暂停";
                    //    _isDownloading = true;
                    //}
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
                var flag = await di.Pause();
                if (flag && _isDownloading)
                {
                    BeginNextCommand.Execute(di);
                }
            });
        }

        public ParamCommand<DownloadItem> ReStartCommand
        {
            get => new ParamCommand<DownloadItem>(async (di) =>
            {
                if (di.DLStatus == DownloadStatus.Downloading)
                {
                    await di.Pause();
                    di.Start();
                }
                else
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
                    var flag = await di.Cancel();
                    if (flag)
                    {
                        BeginNextCommand.Execute(di);
                    }
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
