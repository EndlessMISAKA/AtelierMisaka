using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class CLastDate
    {
        private int _id = 0;
        private string _cId = string.Empty;
        private SiteType _site = SiteType.Fanbox;
        private DateTime _lastDate = DateTime.Now;

        public CLastDate() { }

        public CLastDate(string cid, SiteType site, DateTime ld)
        {
            _cId = cid;
            _site = site;
            _lastDate = ld;
        }

        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get => _id;
            set { _id = value; }
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

        public SiteType Site
        {
            get => _site;
            set
            {
                if (_site != value)
                {
                    _site = value;
                }
            }
        }

        public DateTime LastDate
        {
            get => _lastDate;
            set
            {
                if (_lastDate != value)
                {
                    _lastDate = value;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }
            else if (obj is CLastDate cld)
            {
                return cld.CId == _cId;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
