using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    #region Artist

    public class JsonData_Patreon_Artist
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Attributes attributes { get; set; }
        public string id { get; set; }
    }

    public class Attributes
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    #endregion

    #region Post

    public class JsonData_Patreon_Post
    {
        public Datum[] data { get; set; }
        public Included[] included { get; set; }
        public Meta meta { get; set; }
    }

    public class Meta
    {
        public Pagination pagination { get; set; }
    }

    public class Pagination
    {
        public Cursors cursors { get; set; }
        public int total { get; set; }
    }

    public class Cursors
    {
        public string next { get; set; }
    }

    public class Datum
    {
        public Attributes1 attributes { get; set; }
        public string id { get; set; }
        public Relationships relationships { get; set; }
        public string type { get; set; }
    }

    public class Attributes1
    {
        public string content { get; set; }
        public string content_teaser_text { get; set; }
        public bool current_user_can_view { get; set; } = false;
        public bool current_user_has_liked { get; set; } = false;
        public Embed embed { get; set; }
        public Image image { get; set; }
        public string post_type { get; set; }
        public string published_at { get; set; }
        public string title { get; set; }
        public string url { get; set; }
    }

    public class Embed
    {
        public string url { get; set; }
        public string provider { get; set; }
    }

    public class Image
    {
        public string thumb_url { get; set; }
    }

    public class Relationships
    {
        public Media media { get; set; }
    }

    public class Media
    {
        public Datum1[] data { get; set; }
    }

    public class Datum1
    {
        public string id { get; set; }
    }

    public class Included
    {
        public Attributes2 attributes { get; set; }
        public string id { get; set; }
    }

    public class Attributes2
    {
        public string download_url { get; set; }
        public string file_name { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public Image_Urls image_urls { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Image_Urls
    {
        public string original { get; set; }
    }

    public class Metadata
    {
        public Dimensions dimensions { get; set; }
    }

    public class Dimensions
    {
        public int h { get; set; }
        public int w { get; set; }
    }
    
    #endregion


    #region Pledge

    public class JsonData_Patreon_Pledge
    {
        public Datum2[] data { get; set; }
        public Included2[] included { get; set; }
    }

    public class Datum2
    {
        public Attributes3 attributes { get; set; }
        public Relationships1 relationships { get; set; }
        public string type { get; set; }
    }

    public class Attributes3
    {
        public int amount_cents { get; set; }
        public int pledge_cap_cents { get; set; }
    }

    public class Relationships1
    {
        public Campaign campaign { get; set; }
    }

    public class Campaign
    {
        public Data1 data { get; set; }
    }

    public class Data1
    {
        public string id { get; set; }
    }

    public class Included2
    {
        public Attributes4 attributes { get; set; }
        public string id { get; set; }
    }

    public class Attributes4
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    
    #endregion
}
