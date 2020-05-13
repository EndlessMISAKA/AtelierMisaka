using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class PatreonItem : BaseItem
    {
        public string PID
        {
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string PLink
        {
            set
            {
                if (_link != value)
                {
                    _link = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string FileCount
        {
            get => $"文件数: {_contentUrls.Count} 个";
        }
    }
}
