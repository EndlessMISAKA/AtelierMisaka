﻿using AtelierMisaka.Models;
using System;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public abstract class BaseUtils
    {
        public abstract Task<ResultMessage> CheckCookies();

        public abstract Task<ResultMessage> GetArtistInfo(string url);

        public abstract Task<ResultMessage> GetArtistList();

        public abstract bool GetCover(BaseItem bi);

        public abstract Task<ResultMessage> GetPostIDs(string uid);

        public abstract Task<ResultMessage> LikePost(string pid, string cid);

        protected void Wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            BaseItem bi = (BaseItem)e.UserState;
            bi.Percent = e.ProgressPercentage;
        }

        protected void Wc_DownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs e)
        {
            BaseItem bi = (BaseItem)e.UserState;
            if (e.Cancelled || e.Error != null)
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
