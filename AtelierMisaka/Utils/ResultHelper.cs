using AtelierMisaka.Models;

namespace AtelierMisaka
{
    public static class ResultHelper
    {
        public static ResultMessage WebError(string msg = null)
        {
            string s = msg ?? GlobalLanguage.Msg_ErrorWeb;
            return new ResultMessage(ErrorType.Web, s);
        }

        public static ResultMessage CookieError(string msg = null)
        {
            string s = msg ?? GlobalLanguage.Msg_ErrorCookies;
            return new ResultMessage(ErrorType.Cookies, s);
        }

        public static ResultMessage IOError(string msg = null)
        {
            string s = msg ?? GlobalLanguage.Msg_ErrorIO;
            return new ResultMessage(ErrorType.IO, s);
        }

        public static ResultMessage PathError(string msg = null)
        {
            string s = msg ?? GlobalLanguage.Msg_ErrorPath;
            return new ResultMessage(ErrorType.Path, s);
        }

        public static ResultMessage SecurityError(string msg = null)
        {
            string s = msg ?? GlobalLanguage.Msg_ErrorSecurity;
            return new ResultMessage(ErrorType.Security, s);
        }

        public static ResultMessage UnKnownError(string msg = null)
        {
            string s = msg ?? GlobalLanguage.Msg_ErrorUnKnown;
            return new ResultMessage(ErrorType.UnKnown, s);
        }

        public static ResultMessage NoError(object ret)
        {
            return new ResultMessage(ret);
        }
    }
}
