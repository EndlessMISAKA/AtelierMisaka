using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AtelierMisaka_FileManager
{
    public class NotifyModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            //得到一个副本以预防线程问题
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
