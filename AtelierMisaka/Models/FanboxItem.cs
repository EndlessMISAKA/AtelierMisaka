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
                    _link = $"https://www.fanbox.cc/@{GlobalData.VM_MA.Artist.Cid}/posts/{_id}";
                    RaisePropertyChanged();
                }
            }
        }

        public string FileCount
        {
            get => string.Format(GlobalLanguage.Text_FileImgCou, _contentUrls.Count, _mediaUrls.Count);
        }
    }
}
