using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class CLastDateDic : Dictionary<SiteType, List<CLastDate>>
    {
        SQLiteCLastDateHelper _sqlite = SQLiteCLastDateHelper.Instance;
        CLastDate _current = null;

        public CLastDateDic() : base()
        {
            Add(SiteType.Fanbox, new List<CLastDate>());
            Add(SiteType.Fantia, new List<CLastDate>());
            Add(SiteType.Patreon, new List<CLastDate>());
            LoadData();
        }

        public void LoadData()
        {
            var temp = _sqlite.GetAll();
            if (null != temp)
            {
                foreach (var item in _sqlite.GetAll())
                {
                    base[item.Key] = item.Value;
                }
            }
        }

        public bool TryGetValue(SiteType st, string cid, out DateTime dt)
        {
            var cd = base[st].Find(x => x.CId == cid);
            bool flag = null != cd;
            dt = flag ? cd.LastDate : DateTime.Now;
            return flag;
        }

        public bool Add(CLastDate cld)
        {
            int index = -1;
            if ((index = base[cld.Site].IndexOf(cld)) != -1)
            {
                _current = base[cld.Site][index];
                _current.LastDate = cld.LastDate;
                return true;
            }
            if (_sqlite.InsertDate(cld) > 0)
            {
                base[cld.Site].Add(cld);
                _current = cld;
                return true;
            }
            return false;
        }

        public bool Update(DateTime dt)
        {
            _current.LastDate = dt;
            return (_sqlite.UpdateDate(_current) > 0);
        }
    }
}
