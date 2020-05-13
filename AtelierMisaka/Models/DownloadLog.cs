using SQLite;
using System;

namespace AtelierMisaka.Models
{
    public class DownloadLog
    {
        private int _id = 0;
        private SiteType _site = SiteType.Fanbox;
        private string _cId = string.Empty;
        private string _pId = string.Empty;
        private string _url = string.Empty;
        private string _savePath = string.Empty;
        private string _fileName = string.Empty;
        private DateTime _dlTime = DateTime.Now;
        
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get => _id;
            set { _id = value; }
        }
        
        public SiteType Site
        {
            get => _site;
            set { _site = value; }
        }

        [MaxLength(20)]
        public string CId
        {
            get => _cId;
            set
            {
                if (_cId != value)
                {
                    _cId = value;
                }
            }
        }

        [MaxLength(20)]
        public string PId
        {
            get => _pId;
            set
            {
                if (_pId != value)
                {
                    _pId = value;
                }
            }
        }

        [MaxLength(500)]
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                }
            }
        }

        [MaxLength(255)]
        public string SavePath
        {
            get => _savePath;
            set
            {
                if (_savePath != value)
                {
                    _savePath = value;
                }
            }
        }

        [MaxLength(100)]
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                }
            }
        }

        public DateTime DlTime
        {
            get => _dlTime;
            set
            {
                if (_dlTime != value)
                {
                    _dlTime = value;
                }
            }
        }
    }
}
