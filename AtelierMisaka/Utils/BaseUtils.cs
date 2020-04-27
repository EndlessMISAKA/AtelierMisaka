using AtelierMisaka.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public abstract class BaseUtils
    {
        public abstract ErrorType GetPostIDs(string uid, out IList<BaseItem> bis);

        protected abstract string GetUrls(string jsondata, IList<BaseItem> biList);

        protected abstract void GetPostIDs_Next(string url, IList<BaseItem> biList);

        public abstract ArtistInfo GetArtistInfos(string url);

        public abstract List<ArtistInfo> GetArtistList();

        public abstract bool GetCover(BaseItem bi);

        public static JObject ConvertJSON(string jsondata)
        {
            try
            {
                return (JObject)JsonConvert.DeserializeObject(jsondata);
            }
            catch
            {
                return null;
            }
        }
    }
}
