using AtelierMisaka.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AtelierMisaka.Views
{
    /// <summary>
    /// Pop_Document.xaml 的交互逻辑
    /// </summary>
    public partial class Pop_Document : UserControl
    {
        private bool _mouseD = false;
        private Point _mouM = new Point(0, 0);

        public Pop_Document()
        {
            InitializeComponent();
        }

        public void LoadData(BaseItem bi)
        {
            MainBody.Inlines.Clear();
            var doms = bi.Comments;
            string link = string.Empty;
            bool flag = false;
            for(int i = 0; i < doms.Count; i++)
            {
                if (doms[i].Length == 0)
                {
                    MainBody.Inlines.Add(new LineBreak());
                }
                else if (doms[i].StartsWith("<文件：") || doms[i].StartsWith("<图片："))
                {
                    MainBody.Inlines.Add(new Run(doms[i]) { Foreground = Brushes.LightSeaGreen });
                    MainBody.Inlines.Add(new LineBreak());
                }
                else
                {
                    var ts = doms[i].Split(new string[] { "\n" }, StringSplitOptions.None);
                    foreach (var dis in ts)
                    {
                        var index = dis.IndexOf("http");
                        if (index != -1)
                        {
                            var i2 = dis.IndexOf(' ', index);
                            flag = i2 != -1;
                            if (flag)
                            {
                                link = dis.Substring(index, i2 - index);
                            }
                            else
                            {
                                link = dis.Substring(index);
                            }
                            Hyperlink hl = new Hyperlink(new Run(link))
                            {
                                Command = GlobalData.OpenBrowserCommand,
                                CommandParameter = link
                            };
                            if (index != 0)
                            {
                                MainBody.Inlines.Add(new Run(dis.Substring(0, index)));
                            }
                            MainBody.Inlines.Add(hl);
                            if (flag && i2 != dis.Length - 1)
                            {
                                MainBody.Inlines.Add(new Run(dis.Substring(i2)));
                            }
                        }
                        else
                        {
                            var rr = new Run(dis);
                            if (dis.Length > 0)
                            {
                                if (dis[0] == '$')
                                {
                                    rr.FontSize = 30;
                                }
                                else if (dis.StartsWith("<引用"))
                                {
                                    rr.Foreground = Brushes.LightSkyBlue;
                                }
                            }
                            MainBody.Inlines.Add(rr);
                        }
                        MainBody.Inlines.Add(new LineBreak());
                    }
                }
            }

            if (bi.ContentUrls.Count > 0)
            {
                MainBody.Inlines.Add(new Run("------------------------------------------------------------------------------------------"));
                MainBody.Inlines.Add(new LineBreak());
                MainBody.Inlines.Add(new Run("文件列表: ") { FontSize = 25 });
                MainBody.Inlines.Add(new LineBreak());

                for (int i = 0; i< bi.ContentUrls.Count; i++)
                {
                    Hyperlink hl = new Hyperlink(new Run(bi.FileNames[i]))
                    {
                        Command = GlobalData.DownloadCommand,
                        CommandParameter = new object[] { true, bi, i }
                    };
                    MainBody.Inlines.Add(hl);
                    MainBody.Inlines.Add(new LineBreak());
                }
            }

            if (bi.MediaUrls.Count > 0)
            {
                MainBody.Inlines.Add(new Run("------------------------------------------------------------------------------------------"));
                MainBody.Inlines.Add(new LineBreak());
                MainBody.Inlines.Add(new Run("图片列表: ") { FontSize = 25 });
                MainBody.Inlines.Add(new LineBreak());

                for (int i = 0; i < bi.MediaUrls.Count; i++)
                {
                    Hyperlink hl = new Hyperlink(new Run(bi.MediaNames[i]))
                    {
                        Command = GlobalData.DownloadCommand,
                        CommandParameter = new object[] { false, bi, i }
                    };
                    MainBody.Inlines.Add(hl);
                    MainBody.Inlines.Add(new LineBreak());
                }
            }
        }

        #region Check
        
        private void Btn_Check_Click(object sender, RoutedEventArgs e)
        {
            GlobalData.VM_MA.ShowCheck = false;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
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
