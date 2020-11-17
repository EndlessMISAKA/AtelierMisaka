using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace AtelierMisaka_FileManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Regex rex = new Regex(@"^\d{6}\\\d{2}_\d{4}");
        private Storyboard _sbLoad = null;
        private bool _mouseD = false;
        private Point _mouM = new Point(0, 0);
        FolderSelector fs = new FolderSelector();

        public MainWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
            _sbLoad = (Storyboard)TryFindResource("sb_path");
        }

        #region TitleButton

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VM_MA.MLeft = (ActualWidth - 550) / 2;
            VM_MA.MTop = (ActualHeight - 300) / 2;
        }

        private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode != ResizeMode.NoResize;
        }

        private async void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            VM_MA.Messages = GlobalLanguage.Msg_ExitApp;
            while (VM_MA.ShowCheck)
            {
                await Task.Delay(200);
            }
            if (GlobalData.CheckResult == false)
            {
                return;
            }
            Application.Current.Shutdown();
        }

        private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        #endregion

        #region Check

        private void Btn_Check_Click(object sender, RoutedEventArgs e)
        {
            GlobalData.CheckResult = true;
            VM_MA.ShowCheck = false;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            GlobalData.CheckResult = false;
            VM_MA.ShowCheck = false;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseD = true;
            _mouM = e.GetPosition(cas);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseD)
            {
                var mm = e.GetPosition(cas);
                VM_MA.MLeft += (mm.X - _mouM.X);
                VM_MA.MTop += (mm.Y - _mouM.Y);
                _mouM = mm;
            }
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mouseD = false;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseD = false;
        }

        #endregion

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Interop.HwndSource source = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            if (fs.ShowDialog(win) == System.Windows.Forms.DialogResult.OK)
            {
                VM_MA.SavePath = fs.DirectoryPath;
            }
        }

        private async void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(VM_MA.SavePath) || Directory.Exists(VM_MA.SavePath))
            {
                await GetCheck(GlobalLanguage.Msg_ErrorPath);
                return;
            }
            ShowLoading(true);
            await Task.Run(async () =>
            {
                try
                {
                    var dirs = Directory.GetDirectories(VM_MA.SavePath, "*", SearchOption.TopDirectoryOnly);
                    Match ma = null;
                    string newFileName = string.Empty;
                    foreach (var dr in dirs)
                    {
                        if (VM_MA.SelectedMode == ModeType.AllFolder)
                        {
                            DirectoryInfo di = new DirectoryInfo(dr);
                            var dis = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
                            foreach (var ti in dis)
                            {
                                ti.MoveTo(Path.Combine(VM_MA.SavePath, $"{di.Name}{ti.Name}"));
                            }
                        }
                        else
                        {
                            var fis = Directory.GetFiles(dr, "*.*", SearchOption.AllDirectories).ToList();
                            if (!VM_MA.UseDocumentStr)
                            {
                                fis = fis.Where(x => !Path.GetFileName(x).Equals("Comment.txt")).ToList();
                            }
                            foreach (var fi in fis)
                            {
                                var tp = Path.GetDirectoryName(fi);
                                var temp = tp.Remove(0, VM_MA.SavePath.Length + 1);
                                ma = rex.Match(temp);
                                if (ma.Success)
                                {
                                    if (VM_MA.UseTitleStr)
                                    {
                                        newFileName = Path.Combine(VM_MA.SavePath, $"{temp.Replace("\\", "_")}_{Path.GetFileName(fi)}");
                                    }
                                    else
                                    {
                                        newFileName = Path.Combine(VM_MA.SavePath, $"{ma.Groups[0].Value.Replace("\\", "_")}_{Path.GetFileName(fi)}");
                                    }
                                    File.Move(fi, newFileName);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await GetCheck(GlobalLanguage.Msg_ErrorIO, ex.Message);
                    GlobalData.ErrorLog(ex.Message + Environment.NewLine + ex.StackTrace);
                }
            });
            ShowLoading(false);
        }

        private async void Btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            VM_MA.Messages = GlobalLanguage.Msg_ExitApp;
            while (VM_MA.ShowCheck)
            {
                await Task.Delay(200);
            }
            if (GlobalData.CheckResult == false)
            {
                return;
            }
            Application.Current.Shutdown();
        }

        private async Task<bool> GetCheck(params string[] msgs)
        {
            string mss = msgs[0];
            for (int i = 1; i < msgs.Length; i++)
            {
                mss += Environment.NewLine;
                mss += msgs[i];
            }
            VM_MA.Messages = mss;

            VM_MA.MLeft = (ActualWidth - 550) / 2;
            VM_MA.MTop = (ActualHeight - 300) / 2;
            while (VM_MA.ShowCheck)
            {
                await Task.Delay(500);
            }
            return true;
        }

        private void ShowLoading(bool flag)
        {
            VM_MA.ShowLoad = flag;
            if (flag)
            {
                _sbLoad?.Begin();
            }
            else
            {
                _sbLoad?.Stop();
            }
        }

        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            IntPtr _handle;
            public OldWindow(IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members
            IntPtr System.Windows.Forms.IWin32Window.Handle
            {
                get { return _handle; }
            }
            #endregion
        }
    }
}
