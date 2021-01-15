using AtelierMisaka.Models;
using HTMLConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

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
                    if (GlobalData.CurrentSite == SiteType.Patreon)
                    {
                        if (doms[i].StartsWith(_preLink))
                        {
                            ChangeHttp(doms[i], true);
                        }
                        else
                        {
                            var ss = HtmlToXamlConverter.ConvertHtmlToXaml(doms[i].Replace("\\n", "<br/>"), true);
                            var fd = XamlReader.Load(new XmlTextReader(new StringReader(ss))) as FlowDocument;
                            Paragraph pd = fd.Blocks.FirstBlock as Paragraph;
                            List<Inline> tli = new List<Inline>();
                            while (null != pd)
                            {
                                try
                                {
                                    foreach (var st in pd.Inlines)
                                    {
                                        if (st is Hyperlink)
                                        {
                                            (st as Hyperlink).Click += Hl_Patreon_Click;
                                        }
                                        tli.Add(st);
                                    }
                                    tli.Add(new LineBreak());
                                    pd = pd.NextBlock as Paragraph;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            for (int l = 0; l < tli.Count; l++)
                            {
                                MainBody.Inlines.Add(tli[l]);
                            }

                        }
                    }
                    else
                    {
                        ChangeHttp(doms[i]);
                        //var ts = doms[i].Split(new string[] { "\n" }, StringSplitOptions.None);
                        //foreach (var dis in ts)
                        //{
                        //    var index = dis.IndexOf("http");
                        //    if (index != -1)
                        //    {
                        //        var i2 = dis.IndexOf(' ', index);
                        //        flag = i2 != -1;
                        //        if (flag)
                        //        {
                        //            link = dis.Substring(index, i2 - index);
                        //        }
                        //        else
                        //        {
                        //            link = dis.Substring(index);
                        //        }
                        //        Hyperlink hl = new Hyperlink(new Run(link))
                        //        {
                        //            CommandParameter = link
                        //        };
                        //        hl.Click += Hl_Click;
                        //        if (index != 0)
                        //        {
                        //            MainBody.Inlines.Add(new Run(dis.Substring(0, index)));
                        //        }
                        //        MainBody.Inlines.Add(hl);
                        //        if (flag && i2 != dis.Length - 1)
                        //        {
                        //            MainBody.Inlines.Add(new Run(dis.Substring(i2)));
                        //        }
                        //    }
                        //    else
                        //    {
                        //        var rr = new Run(dis);
                        //        if (dis.Length > 0)
                        //        {
                        //            if (dis[0] == '$')
                        //            {
                        //                rr.FontSize = 30;
                        //            }
                        //            else if (dis.StartsWith(_preLink))
                        //            {
                        //                rr.Foreground = Brushes.LightSkyBlue;
                        //            }
                        //        }
                        //        MainBody.Inlines.Add(rr);
                        //    }
                        //    MainBody.Inlines.Add(new LineBreak());
                        //}
                    }
                }
            }

            if (!string.IsNullOrEmpty(bi.CoverPic))
            {
                MainBody.Inlines.Add(new Run("------------------------------------------------------------------------------------------"));
                MainBody.Inlines.Add(new LineBreak());
                Hyperlink hl = new Hyperlink(new Run(bi.CoverPicName))
                {
                    CommandParameter = new object[] { true, bi, -1 }
                };
                hl.Click += AddDownload;
                Hyperlink hl1 = new Hyperlink(new Run(GlobalLanguage.Text_UseBrowser))
                {
                    CommandParameter = bi.CoverPic
                };
                hl1.Click += Hl_Click;
                MainBody.Inlines.Add(hl);
                MainBody.Inlines.Add(" (");
                MainBody.Inlines.Add(hl1);
                MainBody.Inlines.Add(")");
                MainBody.Inlines.Add(new LineBreak());
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
                        CommandParameter = new object[] { true, bi, i }
                    };
                    hl.Click += AddDownload;
                    Hyperlink hl1 = new Hyperlink(new Run(GlobalLanguage.Text_UseBrowser))
                    {
                        CommandParameter = bi.ContentUrls[i]
                    };
                    hl1.Click += Hl_Click;
                    MainBody.Inlines.Add(hl);
                    MainBody.Inlines.Add(" (");
                    MainBody.Inlines.Add(hl1);
                    MainBody.Inlines.Add(")");
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
                        CommandParameter = new object[] { false, bi, i }
                    };
                    hl.Click += AddDownload;
                    MainBody.Inlines.Add(hl);
                    Hyperlink hl1 = new Hyperlink(new Run(GlobalLanguage.Text_UseBrowser))
                    {
                        CommandParameter = bi.MediaUrls[i]
                    };
                    hl1.Click += Hl_Click;
                    MainBody.Inlines.Add(hl);
                    MainBody.Inlines.Add(" (");
                    MainBody.Inlines.Add(hl1);
                    MainBody.Inlines.Add(")");
                    MainBody.Inlines.Add(new LineBreak());
                }
            }
        }

        private void ChangeHttp(string dom, bool fl = false)
        {
            string link = string.Empty;
            bool flag = false;
            string _preLink = $"<{GlobalLanguage.Text_LinkPref}";
            var ts = dom.Split(new string[] { "\n" }, StringSplitOptions.None);
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
                        CommandParameter = link
                    };
                    hl.Click += Hl_Click;
                    if (index != 0)
                    {
                        MainBody.Inlines.Add(new Run(dis.Substring(0, index)) { Foreground = fl? Brushes.LightSkyBlue : Brushes.White });
                    }
                    MainBody.Inlines.Add(hl);
                    if (flag && i2 != dis.Length - 1)
                    {
                        MainBody.Inlines.Add(new Run(dis.Substring(i2)) { Foreground = fl ? Brushes.LightSkyBlue : Brushes.White });
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

        private void Hl_Patreon_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
        }

        private void AddDownload(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            GlobalData.VM_DL.AddCommand.Execute(link.CommandParameter);
        }

        private void Hl_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            System.Diagnostics.Process.Start((string)link.CommandParameter);
        }
    }
}
