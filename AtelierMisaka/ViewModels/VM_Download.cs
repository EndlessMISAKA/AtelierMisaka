using AtelierMisaka.Commands;
using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.ViewModels
{
    public class VM_Download : NotifyModel
    {
        private bool _isDownloading = false;
        private bool _isChangeThread = false;
        private bool _isChangeProxy = false;
        private bool _isLoading = false;
        private bool _showCheck = false;
        private bool _isQuest = false;
        private string _btnText = "全部开始";
        private string _savePath = string.Empty;
        private int _threadCount = 2;
        private int _postCount = 0;
        private int _loadCount = 0;

        private List<WebClient> _wClients = null;

        private IList<DownloadItem> _downLoadList = null;
        private IList<DownloadItem> _completedList = null;


        public int ThreadCount
        {
            get => _threadCount;
            set
            {
                if (_threadCount != value)
                {
                    _threadCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int LoadCount
        {
            get => _loadCount;
            set
            {
                if (_loadCount != value)
                {
                    _loadCount = value;
                    RaisePropertyChanged("LoadContent");
                }
            }
        }

        public int PostCount
        {
            get => _postCount;
            set
            {
                if (_postCount != value)
                {
                    _postCount = value;
                    RaisePropertyChanged("LoadContent");
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

        public string LoadContent
        {
            get => $"{_loadCount}/{_postCount}";
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
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

        public List<WebClient> WClients
        {
            get => _wClients;
            set
            {
                if (_wClients != value)
                {
                    _wClients = value;
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
                        int index = 0;
                        for (int i = 0; i < _downLoadList.Count; i++)
                        {
                            if (_downLoadList[i].DLStatus != DownloadStatus.Cancel)
                            {
                                _downLoadList[i].DClient = _wClients[index];
                                index++;
                                if (index >= _wClients.Count)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        BtnText = "全部开始";
                        _wClients.ForEach(x => x.CancelAsync());
                    }
                });
                _isQuest = false;
            },
            () =>
            {
                return !_isQuest && _downLoadList != null && _downLoadList.Count > 0;
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

        public ParamCommand<DownloadItem> ReStartCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                if (di.DLStatus == DownloadStatus.Downloading)
                {
                    di.IsReStart = true;
                    di.DClient.CancelAsync();
                }
                else
                {
                    _downLoadList.Remove(di);
                    _downLoadList.Insert(0, di);
                    di.DLStatus = DownloadStatus.Waiting;
                    if (!_isDownloading)
                    {
                        QuestCommand.Execute(null);
                    }
                }
            });
        }

        public ParamCommand<DownloadItem> CancelCommand
        {
            get => new ParamCommand<DownloadItem>((di) =>
            {
                if (di.DLStatus == DownloadStatus.Cancel)
                    return;

                if (di.DLStatus == DownloadStatus.Downloading)
                {
                    di.DClient.CancelAsync();
                }
                di.DLStatus = DownloadStatus.Cancel;
                _downLoadList.Remove(di);
                _downLoadList.Add(di);
            });
        }

    }
}
