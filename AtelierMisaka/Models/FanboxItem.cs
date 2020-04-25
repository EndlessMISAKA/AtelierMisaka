using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class FanboxItem : BaseItem
    {

        public string PID
        {
            set
            {
                if (_id != value)
                {
                    _id = value;
                    _link = $"https://www.pixiv.net/fanbox/creator/{GlobalData.VM_MA.Artist.Id}/post/{_id}";
                    RaisePropertyChanged();
                }
            }
        }

        public string FileCount
        {
            get => $"文件数: {_contentUrls.Count} 个;  图片数: {_mediaUrls.Count} 张";
        }
    }
}
