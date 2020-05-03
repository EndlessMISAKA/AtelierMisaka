using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AtelierMisaka.Converters
{
    public class EnumToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DownloadStatus ds = (DownloadStatus)value;
            string res = "/AtelierMisaka;component/Resources/Status_Waiting.png";
            switch (ds)
            {
                case DownloadStatus.Completed:
                    res = "/AtelierMisaka;component/Resources/Status_Completed.png";
                    break;
                case DownloadStatus.Error:
                    res = "/AtelierMisaka;component/Resources/Status_Error.png";
                    break;
                case DownloadStatus.Downloading:
                    res = "/AtelierMisaka;component/Resources/Status_Downloading.png";
                    break;
                case DownloadStatus.Cancel:
                    res = "/AtelierMisaka;component/Resources/Status_Cancel.png";
                    break;
                case DownloadStatus.Paused:
                    res = "/AtelierMisaka;component/Resources/Btn_Paused.png";
                    break;
            }
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
