using System;

namespace AtelierMisaka.Models
{
    #region Post

    public class JsonData_Fantia_Post
	{
		public Post post { get; set; }
	}

	public class Post
	{
		public int id { get; set; }
		public string title { get; set; }
		public string comment { get; set; }
		public Thumb thumb { get; set; }
		public string posted_at { get; set; }
		public string converted_at { get; set; }
		public Post_Contents[] post_contents { get; set; }
		public string deadline { get; set; }
		public Links links { get; set; }
	}

	public class Thumb
	{
		public string ogp { get; set; }
		public string original { get; set; }
	}

	public class Plan
	{
		public int price { get; set; }
	}

	public class Links
	{
		public Previous previous { get; set; }
	}

	public class Previous
	{
		public int id { get; set; }
		public string posted_at { get; set; }
		public string converted_at { get; set; }
	}

	public class Post_Contents
	{
		public int id { get; set; }
		public string title { get; set; }
		public string visible_status { get; set; }
		public string category { get; set; }
		public string comment { get; set; }
		public Post_Content_Photos[] post_content_photos { get; set; }
		public Plan plan { get; set; }
		public string filename { get; set; }
		public string download_uri { get; set; }
	}

	public class Post_Content_Photos
	{
		public int id { get; set; }
		public Url url { get; set; }
		public string comment { get; set; }
	}

	public class Url
	{
		public string original { get; set; }
	}

    #endregion

    #region Artist

    public class JsonData_Fantia_Artist
    {
        public Fanclub fanclub { get; set; }
    }

    public class Fanclub
    {
        public string creator_name { get; set; }
    }
    
    #endregion
}
