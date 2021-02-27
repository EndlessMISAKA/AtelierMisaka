namespace AtelierMisaka
{
    public enum BackType
    {
        Main,
        Pop
    }

    public enum SiteType
    {
        Fanbox,
        Fantia,
        Patreon
    }

    public enum ErrorType
    {
        Web,
        IO,
        Cookies,
        Path,
        Security,

        UnKnown,
        NoError,
    }

    public enum DownloadStatus
    {
        Waiting,
        Downloading,
        Paused,
        Completed,
        Error,
        Cancel,
        Common,
        Null,
        WriteFile
    }

    public enum ShowType
    {
        All,
        OnlyZero,
        OnlyHttp
    }

    public enum SupportCulture
    {
        zh_CN,
        ja_JP,
        en_US
    }

    public enum RegexType
    {
        RemoveLastDot,
        ProxyString,

        PatreonCid,
        PatreonEmail,
        PatreonHtmlImg,

        FanboxUrl1,
        FanboxUrl2,
        FanboxCSRF,

        FantiaIdName,
        FantiaPlan,
        FantiaPostId,
        FantiaUrl,
        FantiaDataImage
    }
}
