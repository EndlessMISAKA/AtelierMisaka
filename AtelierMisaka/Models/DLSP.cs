namespace AtelierMisaka.Models
{
    public class DLSP
    {
        public string Savepath = string.Empty;
        public string Link = string.Empty;

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }
            if (obj is DLSP dls)
            {
                return this.Link.Equals(dls.Link);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
