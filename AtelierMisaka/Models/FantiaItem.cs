using System.Collections.Generic;

namespace AtelierMisaka.Models
{
    public class FantiaItem : BaseItem
    {
        private List<string> _fees = new List<string>();
        private List<string> _pTitles = new List<string>();
        private string _deadDate = string.Empty;

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
            get => string.Format(GlobalLanguage.Text_FileCou, _contentUrls.Count);
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

        public string DeadDate
        {
            get => _deadDate;
            set
            {
                if (_deadDate != value)
                {
                    _deadDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is FantiaItem fi)
            {
                return fi.ID == this.ID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
