using System.Windows;
using System.Windows.Input;

namespace AtelierMisaka
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _mouseD = false;
        private Point _mouM = new Point(0, 0);

        public MainWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));

            GlobalData.VM_MA = (ViewModels.VM_Main)DataContext;
            GlobalMethord.Init();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalCommand.BackCommand.Execute(BackType.Main);
            GlobalData.VM_MA.MLeft = (ActualWidth - 550) / 2;
            GlobalData.VM_MA.MTop = (ActualHeight - 300) / 2;

            //for fantia test
            //FantiaUtils fu = new FantiaUtils();
            //GlobalData.DownLP = new Views.Downloader(new System.Collections.Generic.List<Models.BaseItem>());
            //fu.GetUrls(System.IO.File.ReadAllText("1.txt"), new System.Collections.Generic.List<Models.BaseItem>());

            System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(10000);
                GlobalCommand.CheckVersionCommand.Execute(null);
            });
        }

        #region TitleButton

        private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode != ResizeMode.NoResize;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalData.VM_MA.IsOpen)
            {
                GlobalData.VM_MA.IsOpen = false;
            }
            GlobalCommand.ExitCommand.Execute(null);
        }

        private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalData.VM_MA.IsOpen)
            {
                GlobalData.VM_MA.IsOpen = false;
            }
            SystemCommands.MaximizeWindow(this);
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalData.VM_MA.IsOpen)
            {
                GlobalData.VM_MA.IsOpen = false;
            }
            SystemCommands.MinimizeWindow(this);
        }

        private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalData.VM_MA.IsOpen)
            {
                GlobalData.VM_MA.IsOpen = false;
            }
            SystemCommands.RestoreWindow(this);
        }

        #endregion

        #region Check

        private void Btn_Check_Click(object sender, RoutedEventArgs e)
        {
            GlobalData.CheckResult = true;
            GlobalData.VM_MA.ShowCheck = false;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            GlobalData.CheckResult = false;
            GlobalData.VM_MA.ShowCheck = false;
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
                GlobalData.VM_MA.MLeft += (mm.X - _mouM.X);
                GlobalData.VM_MA.MTop += (mm.Y - _mouM.Y);
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
    }
}
