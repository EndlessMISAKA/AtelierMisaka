using AtelierMisaka.Commands;
using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AtelierMisaka.ViewModels
{
    public class VM_Main : NotifyModel
    {
        #region MainWindow

        private int _lZindex = 3;
        private IList<BaseItem> _itemList = null;
        private BaseItem _selectedDocument = null;
        private UserControl _popPage = null;

        public int LZindex
        {
            get => _lZindex;
            set
            {
                if (_lZindex != value)
                {
                    _lZindex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IList<BaseItem> ItemList
        {
            get => _itemList;
            set
            {
                if (_itemList != value)
                {
                    _itemList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public BaseItem SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                if (_selectedDocument != value)
                {
                    _selectedDocument = value;
                    RaisePropertyChanged();
                }
            }
        }

        public UserControl PopPage
        {
            get => _popPage;
            set
            {
                if (_popPage != value)
                {
                    _popPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Setting

        private ArtistInfo _artist = new ArtistInfo();
        private SiteType _site = SiteType.Fanbox;
        private string _cookiesFanbox = string.Empty;
        private string _cookiesPatreon = string.Empty;
        private string _proxyFanbox = string.Empty;
        private string _proxyPatreon = string.Empty;
        private string _date = string.Empty;
        private string _messages = string.Empty;
        private string _savePathFanbox = string.Empty;
        private string _savePathPatreon = string.Empty;
        private bool _useProxyFanbox = true;
        private bool _useProxyPatreon = true;
        private bool _useDate = false;
        private bool _showCheck = false;
        private bool _showLoad = false;
        private bool _isStarted = false;
        private double _mLeft = 0d;
        private double _mTop = 0d;
        private Regex _regex = new Regex(@"^\d+\.\d+\.\d+\.\d+:\d+$");
        private IList<ArtistInfo> _artistListFanbox = new ObservableCollection<ArtistInfo>();
        private IList<ArtistInfo> _artistListPatreon = new ObservableCollection<ArtistInfo>();

        private WebProxy _myProxyFanbox = null;
        private WebProxy _myProxyPatreon = null;
        public DateTime LastDate = DateTime.MinValue;
        
        public SiteType Site
        {
            get => _site;
            set
            {
                if (_site != value)
                {
                    _site = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("ArtistList");
                    RaisePropertyChanged("Cookies");
                    RaisePropertyChanged("UseProxy");
                    RaisePropertyChanged("Proxy");
                    RaisePropertyChanged("SavePath");
                }
            }
        }

        public ArtistInfo Artist
        {
            get => _artist;
            set
            {
                if (_artist != value)
                {
                    _artist = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("HasSelected");
                }
            }
        }

        public IList<ArtistInfo> ArtistList
        {
            get
            {
                return _site == SiteType.Fanbox ? _artistListFanbox : _artistListPatreon;
            }
            set
            {
                if (_site == SiteType.Fanbox)
                {
                    if (_artistListFanbox != value)
                    {
                        _artistListFanbox = value;
                        RaisePropertyChanged();
                    }
                }
                else
                {
                    if (_artistListPatreon != value)
                    {
                        _artistListPatreon = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public bool HasSelected
        {
            get => (_artist != null) && (_artist.AName != "自定义");
        }

        public string Messages
        {
            get => _messages;
            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SavePath
        {
            get => _site == SiteType.Fanbox ? _savePathFanbox : _savePathPatreon;
            set
            {
                if(_site == SiteType.Fanbox)
                {
                    if (_savePathFanbox != value)
                    {
                        _savePathFanbox = value;
                        RaisePropertyChanged();
                    }
                }
                else
                {
                    if (_savePathPatreon != value)
                    {
                        _savePathPatreon = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public string Cookies
        {
            get
            {
                return _site == SiteType.Fanbox ? _cookiesFanbox : _cookiesPatreon;
            }
            set
            {
                if (_site == SiteType.Fanbox)
                {
                    if (_cookiesFanbox != value)
                    {
                        _cookiesFanbox = value;
                        RaisePropertyChanged();
                    }
                }
                else
                {
                    if (_cookiesPatreon != value)
                    {
                        _cookiesPatreon = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public string Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    RaisePropertyChanged();
                    if (string.IsNullOrEmpty(_date))
                    {
                        LastDate = DateTime.MinValue;
                    }
                    else
                    {
                        DateTime.TryParse(_date, out LastDate);
                    }
                }
            }
        }

        public string Proxy
        {
            get => _site == SiteType.Fanbox ? _proxyFanbox : _proxyPatreon;
            set
            {
                if (_site == SiteType.Fanbox)
                {
                    if (_proxyFanbox != value)
                    {
                        _proxyFanbox = value;
                        RaisePropertyChanged();
                        if (_regex.IsMatch(_proxyFanbox))
                        {
                            try
                            {
                                _myProxyFanbox = new WebProxy(_proxyFanbox);
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    if (_proxyPatreon != value)
                    {
                        _proxyPatreon = value;
                        RaisePropertyChanged();
                        if (_regex.IsMatch(_proxyPatreon))
                        {
                            try
                            {
                                _myProxyPatreon = new WebProxy(_proxyPatreon);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        public WebProxy MyProxy
        {
            get => _site == SiteType.Fanbox ? _myProxyFanbox : _myProxyPatreon;
        }

        public bool UseProxy
        {
            get => _site == SiteType.Fanbox ? _useProxyFanbox : _useProxyPatreon;
            set
            {
                if(_site == SiteType.Fanbox)
                {
                    if (_useProxyFanbox != value)
                    {
                        _useProxyFanbox = value;
                        RaisePropertyChanged();
                    }
                }
                else
                {
                    if (_useProxyPatreon != value)
                    {
                        _useProxyPatreon = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public bool IsStarted
        {
            get => _isStarted;
            set
            {
                if (_isStarted != value)
                {
                    _isStarted = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool UseDate
        {
            get => _useDate;
            set
            {
                if (_useDate != value)
                {
                    _useDate = value;
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

        public bool ShowLoad
        {
            get => _showLoad;
            set
            {
                if (_showLoad != value)
                {
                    _showLoad = value;
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

        #endregion

        #region Document

        public bool _isShowDocument = false;

        public bool IsShowDocument
        {
            get => _isShowDocument;
            set
            {
                if (_isShowDocument != value)
                {
                    _isShowDocument = value;
                    RaisePropertyChanged();
                }
                RaisePropertyChanged("Img_Document");
                RaisePropertyChanged("Title_Document");
                RaisePropertyChanged("Date_Document");
            }
        }

        public byte[] Img_Document
        {
            get
            {
                return IsShowDocument ? _selectedDocument.ImgData : null;
            }
        }

        public string Title_Document
        {
            get
            {
                return IsShowDocument ? _selectedDocument.Title : string.Empty;
            }
        }

        public string Date_Document
        {
            get
            {
                return IsShowDocument ? _selectedDocument.CreateDate.ToString("yyyy/MM/dd HH:mm") : string.Empty;
            }
        }

        #endregion
    }
}
