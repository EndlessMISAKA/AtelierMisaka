namespace AtelierMisaka_FileManager
{
    public class VM_Main : NotifyModel
    {
        #region MainWindow

        private double _mLeft = 0d;
        private double _mTop = 0d;
        private System.Windows.Controls.UserControl _popPage = null;

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

        private ModeType _mode = ModeType.AllInOne;

        private string _date = string.Empty;
        private string _messages = string.Empty;
        private string _savePath = string.Empty;
        private bool _useDocumentStr = false;
        private bool _showCheck = false;
        private bool _showLoad = false;
        private bool _canChangeLang = true;

        public ModeType SelectedMode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    RaisePropertyChanged();
                }
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

        public bool UseDocumentStr
        {
            get => _useDocumentStr;
            set
            {
                if (_useDocumentStr != value)
                {
                    _useDocumentStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}
