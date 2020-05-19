using AtelierMisaka.Models;
using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AtelierMisaka.Views
{
    /// <summary>
    /// Pop_Document.xaml 的交互逻辑
    /// </summary>
    public partial class Pop_Document : UserControl
    {
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
            string _preFile = $"<{GlobalLanguage.Text_FilePref}";
            string _preImage = $"<{GlobalLanguage.Text_ImagePref}";
            string _preLink = $"<{GlobalLanguage.Text_LinkPref}";
            for (int i = 0; i < doms.Count; i++)
            {
                if (doms[i].Length == 0)
                {
                    MainBody.Inlines.Add(new LineBreak());
                }
                else if (doms[i].StartsWith(_preFile) || doms[i].StartsWith(_preImage))
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
                                Command = GlobalCommand.OpenBrowserCommand,
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
                                else if (dis.StartsWith(_preLink))
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
                MainBody.Inlines.Add(new Run(GlobalLanguage.Text_FList) { FontSize = 25 });
                MainBody.Inlines.Add(new LineBreak());

                for (int i = 0; i< bi.ContentUrls.Count; i++)
                {
                    Hyperlink hl = new Hyperlink(new Run(bi.FileNames[i]))
                    {
                        Command = GlobalData.VM_DL.AddCommand,
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
                MainBody.Inlines.Add(new Run(GlobalLanguage.Text_IList) { FontSize = 25 });
                MainBody.Inlines.Add(new LineBreak());

                for (int i = 0; i < bi.MediaUrls.Count; i++)
                {
                    Hyperlink hl = new Hyperlink(new Run(bi.MediaNames[i]))
                    {
                        Command = GlobalData.VM_DL.AddCommand,
                        CommandParameter = new object[] { false, bi, i }
                    };
                    MainBody.Inlines.Add(hl);
                    MainBody.Inlines.Add(new LineBreak());
                }
            }
        }
    }
}
