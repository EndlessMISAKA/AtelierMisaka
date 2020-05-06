using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class FantiaItem : BaseItem
    {
        private List<string> _fees = new List<string>();
        private List<string> _pTitles = new List<string>();

        public string FID
        {
            set
            {
                if (_id != value)
                {
                    _id = value;
                    _link = $"https://fantia.jp/posts/{_id}";
                    RaisePropertyChanged();
                }
            }
        }

        public string FileCount
        {
            get => $"文件数: {_contentUrls.Count} 个";
        }

        public List<string> Fees
        {
            get => _fees;
            set
            {
                if (_fees != value)
                {
                    _fees = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<string> PTitles
        {
            get => _pTitles;
            set
            {
                if (_pTitles != value)
                {
                    _pTitles = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
