using AtelierMisaka.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AtelierMisaka.Views
{
    /// <summary>
    /// Pop_Document.xaml 的交互逻辑
    /// </summary>
    public partial class Pop_Document : UserControl
    {
        //Regex re = new Regex(@"(http|https)://");

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
                        MainBody.Inlines.Add(new Run(dis));
                    }
                    MainBody.Inlines.Add(new LineBreak());
                }
            }

            if (bi.ContentUrls.Count > 0)
            {
                MainBody.Inlines.Add(new Run("文件列表: "));
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
                MainBody.Inlines.Add(new Run("图片列表: "));
                MainBody.Inlines.Add(new LineBreak());

                for (int i = 0; i < bi.MediaUrls.Count; i++)
                {
                    Hyperlink hl = new Hyperlink(new Run(bi.MediaNames[i]))
                    {
                        Command = GlobalData.DownloadCommand,
                        CommandParameter = new object[] { true, bi, i }
                    };
                    MainBody.Inlines.Add(hl);
                    MainBody.Inlines.Add(new LineBreak());
                }
            }
        }
    }
}
