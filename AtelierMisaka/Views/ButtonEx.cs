using System.Windows;
using System.Windows.Controls;

namespace AtelierMisaka.Views
{
    public class ButtonEx : Button
    {
        public DownloadStatus DLS
        {
            get => (DownloadStatus)GetValue(DLSProperty);
            set => SetValue(DLSProperty, value);
        }

        public static DependencyProperty DLSProperty = DependencyProperty.Register(nameof(DLS), typeof(DownloadStatus), typeof(ButtonEx), new PropertyMetadata(DownloadStatus.Null));

        public string OpenType
        {
            get => (string)GetValue(OpenTypeProperty);
            set => SetValue(OpenTypeProperty, value);
        }

        public static DependencyProperty OpenTypeProperty = DependencyProperty.Register(nameof(OpenType), typeof(string), typeof(ButtonEx), new PropertyMetadata(""));

    }
}
