using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace AtelierMisaka.ViewModels
{
    public class VM_Main : NotifyModel
    {
        #region MainWindow

        private double _mLeft = 0d;
        private double _mTop = 0d;
        private int _lZindex = 3;
        private ShowType _showList = ShowType.All;
        private IList<BaseItem> _itemList = null;
        private IList<BaseItem> _itemList_Zero = null;
        private IList<BaseItem> _itemList_Link = null;
        private BaseItem _selectedDocument = null;
        private System.Windows.Controls.UserControl _popPage = null;

        public ShowType ShowList
        {
            get => _showList;
            set
            {
                if (_showList != value)
                {
                    _showList = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("ItemList");
                }
            }
        }

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
            get
            {
                switch (_showList)
                {
                    case ShowType.OnlyZero:
                        return _itemList_Zero;
                    case ShowType.OnlyHttp:
                        return _itemList_Link;
                    default:
                        return _itemList;
                }
            }
            set
            {
                if (_itemList != value)
                {
                    _itemList = value;
                    _itemList_Zero = _itemList.Where(x => x.IsZero).ToList();
                    _itemList_Link = _itemList.Where(x => x.HasLink).ToList();
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

        public System.Windows.Controls.UserControl PopPage
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

        #region Setting

        private SiteType _site = SiteType.Fanbox;

        private ArtistInfo _artistFanbox = new ArtistInfo();
        private string _cookiesFanbox = string.Empty;
        private string _proxyFanbox = string.Empty;
        private string _savePathFanbox = string.Empty;
        private bool _useProxyFanbox = false;

        private ArtistInfo _artistPatreon = new ArtistInfo();
        private string _cookiesPatreon = string.Empty;
        private string _proxyPatreon = string.Empty;
        private string _savePathPatreon = string.Empty;
        private bool _useProxyPatreon = false;

        private ArtistInfo _artistFantia = new ArtistInfo();
        private string _cookiesFantia = string.Empty;
        private string _proxyFantia = string.Empty;
        private string _savePathFantia = string.Empty;
        private bool _useProxyFantia = false;

        private string _dateS = string.Empty;
        private string _dateE = string.Empty;
        private string _messages = string.Empty;
        private bool _checkFile = false;
        private bool _useDate = false;
        private bool _showCheck = false;
        private bool _showLoad = false;
        private bool _showLogin = false;
        private bool _isStarted = false;
        private bool _canChangeLang = true;
        private Regex _regex = new Regex(@"^\d+\.\d+\.\d+\.\d+:\d+$");
        private IList<ArtistInfo> _artistListFanbox = new ObservableCollection<ArtistInfo>();
        private IList<ArtistInfo> _artistListPatreon = new ObservableCollection<ArtistInfo>();
        private IList<ArtistInfo> _artistListFantia = new ObservableCollection<ArtistInfo>();

        private WebProxy _myProxyFanbox = null;
        private WebProxy _myProxyPatreon = null;
        private WebProxy _myProxyFantia = null;
        private WebProxy _myProxySystem = null;
        private string _proxySystem = string.Empty;

        private object _patreonBrowser = null;
        private bool _isInitialized = false;
        private bool _isOpen = false;
        private bool _isProxyError = false;

        private int _postCount = 0;
        private string _postTitle = string.Empty;

        private string _latestVer = "Check";

        public DateTime LastDate = DateTime.Parse("2010/01/01");
        public DateTime LastDate_End = DateTime.Now;

        public string CurrVersion { get; } = System.Windows.Application.ResourceAssembly.GetName().Version.ToString();

        public string VersionLink = "https://github.com/EndlessMISAKA/AtelierMisaka/releases/latest";
        //public string VersionDL = "https://github.com/EndlessMISAKA/AtelierMisaka/releases/download/V{0}/AtelierMisaka_Soft_{1}.zip";

        public string LatestVersion
        {
            get => _latestVer;
            set
            {
                if (_latestVer != value)
                {
                    _latestVer = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string PostTitle
        {
            get => _postTitle;
            set
            {
                if (_postTitle != value)
                {
                    _postTitle = value;
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                }
            }
        }

        public object PatreonCefBrowser
        {
            get => _patreonBrowser;
            set
            {
                if (_patreonBrowser != value)
                {
                    _patreonBrowser = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                if (_isInitialized != value)
                {
                    _isInitialized = value;
                }
            }
        }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsProxyError
        {
            get => _isProxyError;
            set
            {
                if (_isProxyError != value)
                {
                    _isProxyError = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool CheckFile
        {
            get => _checkFile;
            set
            {
                if(_checkFile != value)
                {
                    _checkFile = value;
                    RaisePropertyChanged();
                    GlobalMethord.SaveNorSet();
                }
            }
        }

        public bool ShowLogin
        {
            get => _showLogin;
            set
            {
                if (_showLogin != value)
                {
                    _showLogin = value;
                    RaisePropertyChanged();
                }
            }
        }

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
                    RaisePropertyChanged("CookieTag");
                    RaisePropertyChanged("PostUrlTag");
                    RaisePropertyChanged("HasSelected");
                    RaisePropertyChanged("Artist");
                }
            }
        }

        public ArtistInfo Artist
        {
            get
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        return _artistFanbox;
                    case SiteType.Fantia:
                        return _artistFantia;
                    default:
                        return _artistPatreon;
                }
            }
            set
            {
                if (null != value)
                {
                    switch (_site)
                    {
                        case SiteType.Fanbox:
                            if (_artistFanbox != value)
                            {
                                _artistFanbox = value;
                            }
                            break;
                        case SiteType.Fantia:
                            if (_artistFantia != value)
                            {
                                _artistFantia = value;
                            }
                            break;
                        default:
                            if (_artistPatreon != value)
                            {
                                _artistPatreon = value;
                            }
                            break;
                    }
                    RaisePropertyChanged();
                    RaisePropertyChanged("HasSelected");
                    if (GlobalData.LastDateDic.TryGetValue(GlobalData.VM_MA.Site, value.Id, out DateTime dt))
                    {
                        Date_Start = dt.ToString("yyyy/MM/dd HH:mm:ss");
                        UseDate = true;
                    }
                    else
                    {
                        if (!ShowLoad)
                        {
                            Date_Start = string.Empty;
                            UseDate = false;
                        }
                    }
                }
            }
        }

        public IList<ArtistInfo> ArtistList
        {
            get
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        return _artistListFanbox;
                    case SiteType.Fantia:
                        return _artistListFantia;
                    default:
                        return _artistListPatreon;
                }
            }
            set
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        if (_artistListFanbox != value)
                        {
                            _artistListFanbox = value;
                        }
                        break;
                    case SiteType.Fantia:
                        if (_artistListFantia != value)
                        {
                            _artistListFantia = value;
                        }
                        break;
                    default:
                        if (_artistListPatreon != value)
                        {
                            _artistListPatreon = value;
                        }
                        break;
                }
                RaisePropertyChanged();
            }
        }

        public int SelectedLang
        {
            get => GlobalData.CurrentCulStr;
            set
            {
                if (!GlobalData.CurrentCulStr.Equals(value))
                {
                    CanChangeLang = false;

                    GlobalData.CurrentCulStr = value;
                    GlobalLanguage.SetCulture(GlobalData.CurrentCulStr);

                    CanChangeLang = true;
                }
            }
        }

        public bool CanChangeLang
        {
            get => _canChangeLang;
            set
            {
                if (_canChangeLang != value)
                {
                    _canChangeLang = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool UpdateCul
        {
            set
            {
                RaisePropertyChanged("CookieTag");
                RaisePropertyChanged("PostUrlTag");
            }
        }

        public string CookieTag
        {
            get
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        return GlobalLanguage.Text_CookiesFanbox;
                    case SiteType.Fantia:
                        return GlobalLanguage.Text_CookiesFantia;
                    default:
                        return GlobalLanguage.Text_CookiesPatreon;
                }
            }
        }

        public string PostUrlTag
        {
            get
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        return GlobalLanguage.Text_CreatorUrlFanbox;
                    case SiteType.Fantia:
                        return GlobalLanguage.Text_CreatorUrlFantia;
                    default:
                        return GlobalLanguage.Text_CreatorUrlPatreon;
                }
            }
        }

        public bool HasSelected
        {
            get => (Artist != null) && !string.IsNullOrEmpty(Artist.Id);
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
                ShowCheck = true;
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
                    if (value)
                    {
                        GlobalData.CheckResult = null;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public string SavePath
        {
            get
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        return _savePathFanbox;
                    case SiteType.Fantia:
                        return _savePathFantia;
                    default:
                        return _savePathPatreon;
                }
            }
            set
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        if (_savePathFanbox != value)
                        {
                            _savePathFanbox = value;
                        }
                        break;
                    case SiteType.Fantia:
                        if (_savePathFantia != value)
                        {
                            _savePathFantia = value;
                        }
                        break;
                    default:
                        if (_savePathPatreon != value)
                        {
                            _savePathPatreon = value;
                        }
                        break;
                }
                RaisePropertyChanged();
            }
        }

        public string Cookies
        {
            get
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        return _cookiesFanbox;
                    case SiteType.Fantia:
                        return _cookiesFantia;
                    default:
                        return _cookiesPatreon;
                }
            }
            set
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        if (_cookiesFanbox != value)
                        {
                            if (!value.StartsWith("FANBOXSESSID="))
                            {
                                _cookiesFanbox = $"FANBOXSESSID={value}";
                            }
                            else
                            {
                                _cookiesFanbox = value;
                            }
                        }
                        break;
                    case SiteType.Fantia:
                        if (_cookiesFantia != value)
                        {
                            if (!value.StartsWith("_session_id="))
                            {
                                _cookiesFantia = $"_session_id={value}";
                            }
                            else
                            {
                                _cookiesFantia = value;
                            }
                        }
                        break;
                    default:
                        if (_cookiesPatreon != value)
                        {
                            _cookiesPatreon = value;
                        }
                        break;
                }
                RaisePropertyChanged();
            }
        }

        public string Date_Start
        {
            get => _dateS;
            set
            {
                if (_dateS != value)
                {
                    _dateS = value;
                    RaisePropertyChanged();
                    if (string.IsNullOrEmpty(_dateS))
                    {
                        LastDate = DateTime.Parse("2010/01/01");
                    }
                    else
                    {
                        if (DateTime.TryParse(_dateS, out DateTime dt))
                        {
                            LastDate = dt;
                        }
                    }
                }
            }
        }

        public string Date_End
        {
            get => _dateE;
            set
            {
                if (_dateE != value)
                {
                    _dateE = value;
                    RaisePropertyChanged();
                    if (string.IsNullOrEmpty(_dateE))
                    {
                        LastDate_End = DateTime.Now;
                    }
                    else
                    {
                        if (DateTime.TryParse(_dateE, out DateTime dt))
                        {
                            LastDate_End = dt;
                        }
                    }
                }
            }
        }

        public string Proxy
        {
            get
            {
                var res = string.Empty;
                switch (_site)
                {
                    case SiteType.Fanbox:
                        res = _proxyFanbox;
                        break;
                    case SiteType.Fantia:
                        res = _proxyFantia;
                        break;
                    default:
                        res = _proxyPatreon;
                        break;
                }
                if (string.IsNullOrEmpty(res))
                {
                    return _proxySystem;
                }
                else
                {
                    return res;
                }
            }
            set
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        if (_proxyFanbox != value)
                        {
                            _proxyFanbox = value;
                            if (_regex.IsMatch(_proxyFanbox))
                            {
                                try
                                {
                                    _myProxyFanbox = new WebProxy(_proxyFanbox);
                                }
                                catch { _myProxyFanbox = null; }
                            }
                            else
                            {
                                _myProxyFanbox = null;
                            }
                            IsProxyError = _myProxyFanbox == null;
                        }
                        break;
                    case SiteType.Fantia:
                        if (_proxyFantia != value)
                        {
                            _proxyFantia = value;
                            if (_regex.IsMatch(_proxyFantia))
                            {
                                try
                                {
                                    _myProxyFantia = new WebProxy(_proxyFantia);
                                }
                                catch { _myProxyFantia = null; }
                            }
                            else
                            {
                                _myProxyFantia = null;
                            }
                            IsProxyError = _myProxyFantia == null;
                        }
                        break;
                    default:
                        if (_proxyPatreon != value)
                        {
                            _proxyPatreon = value;
                            if (_regex.IsMatch(_proxyPatreon))
                            {
                                try
                                {
                                    _myProxyPatreon = new WebProxy(_proxyPatreon);
                                }
                                catch { _myProxyPatreon = null; }
                            }
                            else
                            {
                                _myProxyPatreon = null;
                            }
                            IsProxyError = _myProxyPatreon == null;
                        }
                        break;
                }
                RaisePropertyChanged();
            }
        }

        public string ProxySystem
        {
            set
            {
                if (_proxySystem != value)
                {
                    _proxySystem = value;
                    if (_regex.IsMatch(_proxySystem))
                    {
                        try
                        {
                            _myProxySystem = new WebProxy(_proxySystem);
                        }
                        catch { _myProxySystem = null; }
                    }
                    else
                    {
                        _myProxySystem = null;
                    }
                    RaisePropertyChanged("Proxy");
                }
            }
        }

        public WebProxy MyProxy
        {
            get
            {
                if (string.IsNullOrEmpty(Proxy) || _isProxyError)
                {
                    return _myProxySystem;
                }
                switch (_site)
                {
                    case SiteType.Fanbox:
                        return _myProxyFanbox;
                    case SiteType.Fantia:
                        return _myProxyFantia;
                    default:
                        return _myProxyPatreon;
                }
            }
        }

        public bool UseProxy
        {
            get
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        return _useProxyFanbox;
                    case SiteType.Fantia:
                        return _useProxyFantia;
                    default:
                        return _useProxyPatreon;
                }
            }
            set
            {
                switch (_site)
                {
                    case SiteType.Fanbox:
                        if (_useProxyFanbox != value)
                        {
                            _useProxyFanbox = value;
                        }
                        break;
                    case SiteType.Fantia:
                        if (_useProxyFantia != value)
                        {
                            _useProxyFantia = value;
                        }
                        break;
                    default:
                        if (_useProxyPatreon != value)
                        {
                            _useProxyPatreon = value;
                        }
                        break;
                }
                RaisePropertyChanged();
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
                RaisePropertyChanged("IsLiked_Document");
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

        public string UpDate_Document
        {
            get
            {
                return IsShowDocument ? _selectedDocument.UpdateDate.ToString("yyyy/MM/dd HH:mm") : string.Empty;
            }
        }

        public string DeadDate_Document
        {
            get
            {
                return IsShowDocument ? ((_selectedDocument is FantiaItem) ? (_selectedDocument as FantiaItem).DeadDate : string.Empty) : string.Empty;
            }
        }

        public System.Windows.Visibility Site_Document
        {
            get
            {
                return IsShowDocument ? ((_site == SiteType.Fantia) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed) : System.Windows.Visibility.Collapsed;
            }
        }

        public bool IsLiked_Document
        {
            get
            {
                if (null != _selectedDocument)
                {
                    return _selectedDocument.IsLiked;
                }
                return false;
            }
            set
            {
                if (_selectedDocument.IsLiked != value)
                {
                    _selectedDocument.IsLiked = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}
