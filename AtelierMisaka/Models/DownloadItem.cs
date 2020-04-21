using AtelierMisaka.Commands;
using AtelierMisaka.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class DownloadItem : NotifyModel
    {
        private DownloadStatus _ds = DownloadStatus.Waiting;
        private string _fileName = string.Empty;
        private string _savePath = string.Empty;
        private string _link = string.Empty;
        private DateTime _cTime = DateTime.Now;
        private int _percent = 0;
        private int _reTryC = 0;
        private bool _isCompleted = false;
        private bool _isReStart = false;
        private long _contentLength = 0;
        private long _receviedCount = 0;
        private long _totalRC = 0;
        private int _zeroCount = 0;
        
        private WebClient _dClient = null;
        private Timer _showSpeed = null;

        public BaseItem SourceDocu = null;

        private void ShowSpeed(object state)
        {
            RaisePropertyChanged("SpeedString");
        }

        public DownloadItem()
        {
            _showSpeed = new Timer(ShowSpeed);
        }
        
        public DownloadStatus DLStatus
        {
            get => _ds;
            set
            {
                if (_ds != value)
                {
                    _ds = value;
                    RaisePropertyChanged();
                    if (_ds != DownloadStatus.Downloading)
                    {
                        _showSpeed.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    else
                    {
                        _showSpeed.Change(1000, 1000);
                    }
                }
            }
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
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

        public string Link
        {
            get => _link;
            set
            {
                if (_link != value)
                {
                    _link = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DateTime CTime
        {
            get => _cTime;
            set
            {
                if (_cTime != value)
                {
                    _cTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int ZeroCount
        {
            get => _zeroCount;
            set
            {
                if (value == 30)
                {
                    _dClient.CancelAsync();
                    _zeroCount = 0;
                }
                else
                {
                    _zeroCount = value;
                }
            }
        }

        public int ReTryCount
        {
            get => _reTryC;
            set
            {
                if (_reTryC != value)
                {
                    _reTryC = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int Percent
        {
            get => _percent;
            set
            {
                if (_percent != value)
                {
                    _percent = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("PercentString");
                }
            }
        }

        public string PercentString
        {
            get => $"{_percent}%";
        }

        public string SizeString
        {
            get
            {
                if (_contentLength == -1)
                {
                    return "未知大小";
                }
                if (_contentLength == 0)
                {
                    return "---";
                }
                var re = _contentLength / 1024d;
                var dw = "KB";
                if (re >= 1024)
                {
                    re /= 1024d;
                    dw = "MB";
                    if (re >= 1024)
                    {
                        re /= 1024d;
                        dw = "GB";
                    }
                }
                return $"{re.ToString("#0.00")}{dw}";
            }
        }

        public string SpeedString
        {
            get
            {
                var speed = _totalRC - _receviedCount;
                if (speed == 0)
                {
                    ZeroCount++;
                    return "---";
                }
                _zeroCount = 0;
                var re = speed / 1024d;
                var dw = "KB/s";
                if (re >= 1024)
                {
                    re /= 1024d;
                    dw = "MB/s";
                }
                var res = $"{re.ToString("#0.00")}{dw}";
                Interlocked.Add(ref _receviedCount, speed);
                return res;
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsReStart
        {
            get => _isReStart;
            set
            {
                if (_isReStart != value)
                {
                    _isReStart = value;
                    RaisePropertyChanged();
                }
            }
        }

        public long ContentLength
        {
            get => _contentLength;
            set
            {
                if (_contentLength != value)
                {
                    _contentLength = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("SizeString");
                }
            }
        }

        public long TotalRC
        {
            get => _totalRC;
            set
            {
                if (_totalRC != value)
                {
                    _totalRC = value;
                    RaisePropertyChanged();
                }
            }
        }


        public WebClient DClient
        {
            get => _dClient;
            set
            {
                if (null != value)
                {
                    _dClient = value;
                    _dClient.DownloadDataAsync(new Uri(_link), this);
                    _receviedCount = 0;
                    _totalRC = 0;
                    DLStatus = DownloadStatus.Downloading;
                    //RaisePropertyChanged("DLStatus");
                }
                else
                {
                    _dClient = null;
                }
            }
        }
    }
}
