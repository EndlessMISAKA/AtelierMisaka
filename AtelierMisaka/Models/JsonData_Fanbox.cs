using System;
using System.Collections.Generic;

namespace AtelierMisaka.Models
{
    #region Post

    public class JsonData_Fanbox_Post
	{
		public Body body { get; set; }
	}

	public class Body
	{
		public Item[] items { get; set; }
		public string nextUrl { get; set; }
	}

	public class Item
	{
		public string id { get; set; }
		public string title { get; set; }
		public string coverImageUrl { get; set; }
		public int feeRequired { get; set; }
		public DateTime publishedDatetime { get; set; }
		public DateTime updatedDatetime { get; set; }
		public string type { get; set; }
		public ItemBody body { get; set; }
		public bool isLiked { get; set; }
		public string creatorId { get; set; }
		public string status { get; set; }
	}

	public class ItemBody
	{
		public Block[] blocks { get; set; }
		public Dictionary<string, ImageItem> imageMap { get; set; }
		public Dictionary<string, FileItem> fileMap { get; set; }
		public Dictionary<string, EmbedItem> embedMap { get; set; }
		public ImageItem[] images { get; set; }
		public FileItem[] files { get; set; }
		public string text { get; set; }
	}

	public class ImageItem
	{
		public string id { get; set; }
		public string extension { get; set; }
		public int width { get; set; }
		public int height { get; set; }
		public string originalUrl { get; set; }
	}

	public class FileItem
	{
		public string id { get; set; }
		public string extension { get; set; }
		public string name { get; set; }
		public int size { get; set; }
		public string url { get; set; }
	}

	public class EmbedItem
	{
		public string id { get; set; }
		public string serviceProvider { get; set; }
		public string contentId { get; set; }
	}

	public class Block
	{
		public string type { get; set; }
		public string text { get; set; }
		public string embedId { get; set; }
		public string imageId { get; set; }
		public string fileId { get; set; }
	}

    #endregion

    #region Artist

    public class JsonData_Fanbox_Artist
    {
        public ArtistBody body { get; set; }
    }

    public class ArtistBody
    {
        public User user { get; set; }
        public string creatorId { get; set; }
        public string[] profileLinks { get; set; }
    }

    public class User
    {
        public string userId { get; set; }
        public string name { get; set; }
    }

    #endregion

    #region Plan

    public class JsonData_Fanbox_Plan
    {
        public PlanBody[] body { get; set; }
    }

    public class PlanBody
    {
        public int fee { get; set; }
        public string creatorId { get; set; }
    }

    #endregion
}
