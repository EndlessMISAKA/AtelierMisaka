using AtelierMisaka.Models;
using System;
using System.Collections.Generic;

namespace AtelierMisaka
{
    public abstract class BaseUtils
    {
        public abstract ArtistInfo GetArtistInfo(string url);

        public abstract List<ArtistInfo> GetArtistList();

        public abstract bool GetCover(BaseItem bi);

        public abstract ErrorType GetPostIDs(string uid, out IList<BaseItem> bis);

        public abstract ErrorType LikePost(string pid, string cid);

        protected void Wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            BaseItem bi = (BaseItem)e.UserState;
            bi.Percent = e.ProgressPercentage;
        }

        protected void Wc_DownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs e)
        {
            BaseItem bi = (BaseItem)e.UserState;
            if(e.Cancelled || e.Error != null)
            {
                bi.Percent = -1;
            }
            else
            {
                byte[] data = new byte[e.Result.Length];
                Array.Copy(e.Result, data, e.Result.Length);
                bi.ImgData = data;
            }
            ((System.Net.WebClient)sender).Dispose();
        }
    }
}
