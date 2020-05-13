using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public static class ResultHelper
    {
        public static ResultMessage WebError(string msg = null)
        {
            string s = msg ?? "网络错误";
            return new ResultMessage(ErrorType.Web, s);
        }

        public static ResultMessage CookieError(string msg = null)
        {
            string s = msg ?? "请确认Cookie是否过期";
            return new ResultMessage(ErrorType.Cookies, s);
        }

        public static ResultMessage IOError(string msg = null)
        {
            string s = msg ?? $"无法转换json数据{Environment.NewLine}请联系开发者";
            return new ResultMessage(ErrorType.IO, s);
        }

        public static ResultMessage PathError(string msg = null)
        {
            string s = msg ?? "输入的链接有误";
            return new ResultMessage(ErrorType.Path, s);
        }

        public static ResultMessage SecurityError(string msg = null)
        {
            string s = msg ?? $"认证机制出错{Environment.NewLine}请联系开发者";
            return new ResultMessage(ErrorType.Security, s);
        }

        public static ResultMessage UnKnownError(string msg = null)
        {
            string s = msg ?? "未知错误，请联系开发者";
            return new ResultMessage(ErrorType.UnKnown, s);
        }

        public static ResultMessage NoError(object ret)
        {
            return new ResultMessage(ret);
        }
    }
}
