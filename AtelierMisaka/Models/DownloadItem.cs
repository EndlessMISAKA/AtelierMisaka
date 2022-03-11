using AtelierMisaka.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class DownloadItem : NotifyModel
    {
        private DownloadStatus _ds = DownloadStatus.Waiting;
        private ErrorType _dlRet = ErrorType.UnKnown;
        private string _fileName = string.Empty;
        private string _savePath = string.Empty;
        private string _link = string.Empty;
        private string _fullPath = string.Empty;
        private string _errorMsg = string.Empty;
        private DateTime _cTime = DateTime.Now;
        private int _percent = 0;
        private int _reTryC = 0;
        private bool _isStop = false;
        private long _contentLength = 0;
        private long _receviedCount = 0;
        private long _totalRC = 0;
        private int _zeroCount = 0;
        private byte[] _dlData = null;
        
        private Timer _showSpeed = null;

        private HttpWebRequest _request;

        public BaseItem SourceDocu = null;
        public string AId = string.Empty;
        public bool IsDataImage = false;

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

        public string SavePathStr
        {
            get => _savePath.Replace(GlobalData.VM_DL.SavePath, "{Base Path}");
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

        public string ErrorMsg
        {
            get => _errorMsg;
            set
            {
                if (_errorMsg != value)
                {
                    _errorMsg = value;
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
                if (value == 45)
                {
                    GlobalData.VM_DL.ReStartCommand.Execute(this);
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
                if (_contentLength == 0)
                {
                    return "---";
                }
                if (_contentLength == -1)
                {
                    return GlobalLanguage.Text_UnKnownSize;
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
                return $"{Math.Round(re)}{dw}";
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
                var res = $"{Math.Round(re, 2)}{dw}";
                Interlocked.Add(ref _receviedCount, speed);
                if (_contentLength > 0)
                {
                    Percent = (int)((double)_receviedCount / _contentLength * 100);
                }
                return res;
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

        public void CheckTempFile()
        {
            _fullPath = Path.Combine(_savePath, _fileName);
            var fnt = $"{_fullPath}.msk";
            if (File.Exists(fnt))
            {
                FileInfo fi = new FileInfo(fnt);
                var fs = fi.Open(FileMode.Open);
                _totalRC = fs.Length;
                fs.Close();
            }
        }

        public async void Start()
        {
            if (_ds == DownloadStatus.Waiting || _ds == DownloadStatus.Paused)
            {
                DLStatus = DownloadStatus.Downloading;
                _dlRet = await Task.Run(() =>
                {
                    try
                    {
                        ErrorMsg = string.Empty;
                        _request = WebRequest.CreateHttp(_link);
                        if (GlobalData.VM_MA.UseProxy)
                        {
                            _request.Proxy = GlobalData.VM_MA.MyProxy;
                        }
                        if (GlobalData.VM_DL.TempSite != SiteType.Patreon)
                        {
                            _request.Headers.Add(HttpRequestHeader.Cookie, GlobalData.VM_MA.Cookies);
                        }
                        if (_totalRC != 0)
                        {
                            _request.AddRange("bytes", _totalRC);
                        }
                        HttpWebResponse response = _request.GetResponse() as HttpWebResponse;
                        if (_contentLength == 0)
                        {
                            if (long.TryParse(response.Headers[HttpResponseHeader.ContentLength], out long ll))
                            {
                                ContentLength = ll;
                                if (ll < Int32.MaxValue)
                                {
                                    _dlData = new byte[_contentLength];
                                }
                                else
                                {
                                    _dlData = null;
                                }
                            }
                            else
                            {
                                ContentLength = -1;
                            }
                        }
                        Stream sm = response.GetResponseStream();
                        byte[] arra = new byte[1024];
                        int i = sm.Read(arra, 0, arra.Length);
                        if (_dlData != null)
                        {
                            while (i > 0)
                            {
                                Array.Copy(arra, 0, _dlData, _totalRC, i);
                                Interlocked.Add(ref _totalRC, i);
                                i = sm.Read(arra, 0, arra.Length);
                            }

                            DLStatus = DownloadStatus.WriteFile;
                            if (File.Exists(_fullPath))
                            {
                                int index = 1;
                                var ext = _fileName.Split('.').Last();
                                int iln = ext.Length + 1;
                                string _fname = _fileName.Substring(0, _fileName.Length - iln);
                                FileName = $"{_fname}_{index}.{ext}";
                                _fullPath = Path.Combine(_savePath, _fileName);
                                while (File.Exists(_fullPath))
                                {
                                    FileName = $"{_fname}_{++index}.{ext}";
                                    _fullPath = Path.Combine(_savePath, _fileName);
                                }
                            }
                            File.WriteAllBytes(_fullPath, _dlData);
                            GlobalData.DLLogs.Add(new DownloadLog()
                            {
                                CId = AId,
                                PId = SourceDocu.ID,
                                Url = _link.Split('?').FirstOrDefault(),
                                SavePath = _savePath,
                                FileName = _fileName,
                                Site = GlobalData.VM_DL.TempSite
                            });
                            Array.Clear(_dlData, 0, _dlData.Length);
                            _dlData = null;
                            GC.Collect(9);
                        }
                        else
                        {
                            if (_totalRC == 0 && File.Exists(_fullPath))
                            {
                                int index = 1;
                                var ext = _fileName.Split('.').Last();
                                int iln = ext.Length + 1;
                                string _fname = _fileName.Substring(0, _fileName.Length - iln);
                                FileName = $"{_fname}_{index}.{ext}";
                                _fullPath = Path.Combine(_savePath, _fileName);
                                while (File.Exists(_fullPath))
                                {
                                    FileName = $"{_fname}_{++index}.{ext}";
                                    _fullPath = Path.Combine(_savePath, _fileName);
                                }
                            }
                            FileStream fs = null;
                            if (_contentLength != -1)
                            {
                                try
                                {
                                    fs = new FileStream($"{_fullPath}.msk", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                                    //fs.Seek(_totalRC, SeekOrigin.Begin);
                                    while (i > 0)
                                    {
                                        fs.Write(arra, 0, i);
                                        //Interlocked.Add(ref _totalRC, i);
                                        i = sm.Read(arra, 0, arra.Length);
                                    }
                                    fs.Flush();
                                    fs.Close();
                                }
                                catch
                                {
                                    sm.Dispose();
                                    throw;
                                }
                                finally
                                {
                                    if (null != fs)
                                    {
                                        fs.Close();
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    _totalRC = 0;
                                    _receviedCount = 0;
                                    fs = new FileStream($"{_fullPath}.msk", FileMode.Create, FileAccess.ReadWrite);
                                    while (i > 0)
                                    {
                                        fs.Write(arra, 0, i);
                                        Interlocked.Add(ref _totalRC, i);
                                        i = sm.Read(arra, 0, arra.Length);
                                    }
                                    fs.Flush();
                                    fs.Close();
                                }
                                catch
                                {
                                    sm.Dispose();
                                    throw;
                                }
                                finally
                                {
                                    if (null != fs)
                                    {
                                        fs.Close();
                                    }
                                }
                            }
                            File.Move($"{_fullPath}.msk", _fullPath);
                            GlobalData.DLLogs.Add(new DownloadLog()
                            {
                                CId = AId,
                                PId = SourceDocu.ID,
                                Url = _link.Split('?').FirstOrDefault(),
                                SavePath = _savePath,
                                FileName = _fileName,
                                Site = GlobalData.VM_DL.TempSite
                            });
                            GC.Collect(9);
                        }
                        sm.Dispose();
                        DLStatus = DownloadStatus.Completed;
                        return ErrorType.NoError;
                    }
                    catch (Exception ex)
                    {
                        GlobalMethord.ErrorDownload(ex.Message, this);
                        if (ex is PathTooLongException)
                        {
                            return ErrorType.Path;
                        }
                        else if (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
                        {
                            return ErrorType.Security;
                        }
                        else if (ex.Message.Contains("403"))
                        {
                            return ErrorType.UnKnown;
                        }
                        else if (ex.Message.Contains("404"))
                        {
                            return ErrorType.IO;
                        }
                        else
                        {
                            if (ex.TargetSite.DeclaringType == typeof(Array))
                            {
                                Array.Clear(_dlData, 0, _dlData.Length);
                                _dlData = null;
                                _contentLength = 0;
                                _totalRC = 0;
                                _receviedCount = 0;
                            }
                            return ErrorType.Web;
                        }
                    }
                });
                if (_dlRet == ErrorType.NoError)
                {
                    GlobalData.SyContext.Send((dd) =>
                    {
                        GlobalData.VM_DL.MoveToComLCommand.Execute(dd);
                        GlobalData.VM_DL.BeginNextCommand.Execute(dd);
                    }, this);
                }
                else if (_ds == DownloadStatus.Downloading)
                {
                    DLStatus = DownloadStatus.Error;
                    _request.Abort();
                    if (_dlRet == ErrorType.Web)
                    {
                        ErrorMsg = GlobalLanguage.Msg_DLErrWeb;
                        if (++_reTryC < 10)
                        {
                            DLStatus = DownloadStatus.Waiting;
                            Start();
                        }
                        else
                        {
                            GlobalData.SyContext.Send((dd) =>
                            {
                                GlobalData.VM_DL.BeginNextCommand.Execute(dd);
                            }, this);
                        }
                    }
                    else if (_dlRet == ErrorType.IO)
                    {
                        if (IsDataImage)
                        {
                            ErrorMsg = "HTTP500";
                        }
                        else
                        {
                            ErrorMsg = "HTTP404";
                        }
                        GlobalData.SyContext.Send((dd) =>
                        {
                            GlobalData.VM_DL.BeginNextCommand.Execute(dd);
                        }, this);
                    }
                    else if (_dlRet == ErrorType.UnKnown)
                    {
                        ErrorMsg = "HTTP403";
                        GlobalData.SyContext.Send((dd) =>
                        {
                            GlobalData.VM_DL.AddRetryCommand.Execute(dd);
                            GlobalData.VM_DL.BeginNextCommand.Execute(dd);
                        }, this);
                    }
                    else
                    {
                        if (_dlRet == ErrorType.Path)
                        {
                            ErrorMsg = GlobalLanguage.Msg_DLErrPath;
                        }
                        else
                        {
                            ErrorMsg = GlobalLanguage.Msg_DLErrSecu;
                        }
                        GlobalData.SyContext.Send((dd) =>
                        {
                            GlobalData.VM_DL.BeginNextCommand.Execute(dd);
                        }, this);
                    }
                }
            }
        }

        public async Task<bool> Pause()
        {
            if (_ds == DownloadStatus.Downloading)
            {
                DLStatus = DownloadStatus.Paused;
                //_isStop = true;
                _request.Abort();

                await Task.Delay(100);
                //while (_isStop)
                //{
                //    await Task.Delay(100);
                //}
                //DLStatus = DownloadStatus.Paused;
                return true;
            }
            return false;
        }

        public async Task<bool> Cancel()
        {
            //_isStop = false;
            if (_ds == DownloadStatus.Downloading || _ds == DownloadStatus.Error || _ds == DownloadStatus.Paused)
            {
                DLStatus = DownloadStatus.Cancel;
                //_isStop = true;
                _request.Abort();
                //while (_isStop)
                //{
                //    await Task.Delay(100);
                //}
                await Task.Delay(100);

                if (_contentLength != -1)
                {
                    Array.Clear(_dlData, 0, _dlData.Length);
                    _dlData = null;
                    _contentLength = 0;
                    _totalRC = 0;
                    _receviedCount = 0;
                }
                else
                {
                    var fnt = $"{Path.Combine(_savePath, _fileName)}.msk";
                    if (File.Exists(fnt))
                    {
                        File.Delete(fnt);
                    }
                    _contentLength = 0;
                    _totalRC = 0;
                    _receviedCount = 0;
                }
                GC.Collect(9);
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }
            else if (obj is DownloadItem di)
            {
                return di.Link.Split('?').FirstOrDefault() == _link.Split('?').FirstOrDefault();
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
