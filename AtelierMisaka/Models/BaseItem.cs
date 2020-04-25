﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtelierMisaka.ViewModels;

namespace AtelierMisaka.Models
{
    public class BaseItem : NotifyModel
    {
        protected string _id = string.Empty;
        protected string _title = string.Empty;
        protected string _coverPic = string.Empty;
        protected string _fee = string.Empty;
        protected string _link = string.Empty;
        protected DateTime _createDate = DateTime.Now;
        protected DateTime _updateDate = DateTime.Now;
        protected bool _needLoadCover = true;
        protected bool _skip = false;
        protected byte[] _imgData = null;

        protected List<string> _comments = new List<string>();
        protected List<string> _contentUrls = new List<string>();
        protected List<string> _mediaUrls = new List<string>();
        protected List<string> _fileNames = new List<string>();
        protected List<string> _mediaNames = new List<string>();

        public string ID
        {
            get => _id;
        }

        public bool IsZero
        {
            get => (_contentUrls.Count == 0 && _mediaUrls.Count == 0);
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Link
        {
            get => _link;
        }

        public string Fee
        {
            get => _fee;
            set
            {
                if (_fee != value)
                {
                    _fee = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string CoverTxt
        {
            get
            {
                if (string.IsNullOrEmpty(_coverPic))
                {
                    return "无封面";
                }
                else if (!_needLoadCover)
                {
                    return "加载中";
                }
                else
                {
                    return "点击加载封面图";
                }
            }
        }

        public string CoverPic
        {
            get => _coverPic;
            set
            {
                if (_coverPic != value)
                {
                    _coverPic = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DateTime CreateDate
        {
            get => _createDate;
            set
            {
                if (_createDate != value)
                {
                    _createDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DateTime UpdateDate
        {
            get => _updateDate;
            set
            {
                if (_updateDate != value)
                {
                    _updateDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string CreateDateStr
        {
            get => _createDate.ToString("yyyy/MM/dd HH:mm");
        }

        public List<string> Comments
        {
            get => _comments;
            set
            {
                if (_comments != value)
                {
                    _comments = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool NeedLoadCover
        {
            get => _needLoadCover;
            set
            {
                if (_needLoadCover != value)
                {
                    _needLoadCover = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("CoverTxt");
                }
            }
        }

        public bool Skip
        {
            get => _skip;
            set
            {
                if (_skip != value)
                {
                    _skip = value;
                    RaisePropertyChanged();
                }
            }
        }

        public byte[] ImgData
        {
            get => _imgData;
            set
            {
                if (_imgData != value)
                {
                    _imgData = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<string> ContentUrls
        {
            get => _contentUrls;
            set
            {
                if (_contentUrls != value)
                {
                    _contentUrls = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<string> MediaUrls
        {
            get => _mediaUrls;
            set
            {
                if (_mediaUrls != value)
                {
                    _mediaUrls = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<string> FileNames
        {
            get => _fileNames;
            set
            {
                if (_fileNames != value)
                {
                    _fileNames = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<string> MediaNames
        {
            get => _mediaNames;
            set
            {
                if (_mediaNames != value)
                {
                    _mediaNames = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}