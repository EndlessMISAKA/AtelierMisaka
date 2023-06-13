using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class DownloadLogList : List<DownloadLog>
    {
        SQLiteDownloadLogHelper _sqlite = SQLiteDownloadLogHelper.Instance;

        private string _pid = string.Empty;
        private List<DownloadLog> _selectedLogs = new List<DownloadLog>();

        public bool IsExist { get; private set; } = false;

        public DownloadLogList() : base() { }

        public void LoadData(string cid, SiteType st)
        {
            if (Count > 0)
            {
                Clear();
                _selectedLogs.Clear();
            }
            var all = _sqlite.GetLogs(cid, st);
            if (null != all)
            {
                foreach (var item in all)
                {
                    base.Add(item);
                }
            }
        }

        public void SetPId(string pid)
        {
            if (_pid != pid)
            {
                _pid = pid;
            }
            _selectedLogs = base.FindAll(x => x.PId == _pid);
            IsExist = _selectedLogs.Count > 0;
        }

        public bool HasLog(string link, string folder, string fn)
        {
            if (GlobalData.VM_MA.CheckFile)
            {
                return GlobalMethord.IsFileExist(folder, fn);
            }
            else
            {
                if (IsExist)
                {
                    var dd = _selectedLogs.Find(x => link.Contains(x.Url));
                    return dd != null && dd.SavePath == folder && dd.FileName == fn && GlobalMethord.IsFileExist(folder, fn);
                }
                return false;
            }
        }

        public new bool Add(DownloadLog dl)
        {
            if (_sqlite.InsertLog(dl) != 0)
            {
                base.Add(dl);
                return true;
            }
            return false;
        }

        public async Task<bool> AddRangeAsync(IList<DownloadLog> dls)
        {
            if (await _sqlite.InsertLogs(dls) == 0)
            {
                foreach (var item in dls)
                {
                    base.Add(item);
                }
                return true;
            }
            return false;
        }
    }
}
