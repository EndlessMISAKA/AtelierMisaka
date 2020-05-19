using AtelierMisaka.Commands;
using AtelierMisaka.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public class GlobalCommand
    {

        public static ParamCommand<BackType> BackCommand = new ParamCommand<BackType>((flag) =>
        {
            if (flag == BackType.Main)
            {
                GlobalData.VM_MA.PopPage = GlobalData.Pop_Setting;
                GlobalData.VM_MA.LZindex = 3;
            }
            else
            {
                GlobalData.VM_MA.PopPage = null;
                GlobalData.VM_MA.IsShowDocument = false;
                GlobalData.VM_MA.LZindex = 0;
            }
        });

        public static ParamCommand<BaseItem> ShowDocumentCommand = new ParamCommand<BaseItem>((bi) =>
        {
            GlobalData.VM_MA.SelectedDocument = bi;
            GlobalData.VM_MA.IsShowDocument = true;
            GlobalData.Pop_Document.LoadData(bi);
            GlobalData.VM_MA.PopPage = GlobalData.Pop_Document;
            GlobalData.VM_MA.LZindex = 3;
        });

        public static ParamCommand<BaseItem> GetCoverCommand = new ParamCommand<BaseItem>((bi) =>
        {
            GlobalData.VM_MA.SelectedDocument = bi;
            if (string.IsNullOrEmpty(bi.CoverPic))
            {
                return;
            }
            bi.NeedLoadCover = false;
            GlobalData.CaptureUtil.GetCover(bi);
        });

        public static CommonCommand ExitCommand = new CommonCommand(async () =>
        {
            GlobalData.VM_MA.Messages = GlobalLanguage.Msg_ExitApp;
            while (GlobalData.VM_MA.ShowCheck)
            {
                await Task.Delay(200);
            }
            if (GlobalData.CheckResult == false)
            {
                return;
            }
            System.Windows.Application.Current.Shutdown();
        });

        public static CommonCommand ShowDLCommand = new CommonCommand(() =>
        {
            GlobalData.DownLP.Show();
            GlobalData.DownLP.Activate();
        });

        public static CommonCommand OpenLinkCommand = new CommonCommand(() =>
        {
            System.Diagnostics.Process.Start(GlobalData.VM_MA.SelectedDocument.Link);
        });

        public static ParamCommand<string> OpenBrowserCommand = new ParamCommand<string>((link) =>
        {
            System.Diagnostics.Process.Start(link);
        });

        public static CommonCommand LikePostCommand = new CommonCommand(async () =>
        {
            if (GlobalData.VM_MA.IsLiked_Document)
                return;

            var ret = await GlobalData.CaptureUtil.LikePost(GlobalData.VM_MA.SelectedDocument.ID, GlobalData.VM_MA.Artist.Cid);
            if (ret.Error == ErrorType.NoError)
            {
                GlobalData.VM_MA.IsLiked_Document = true;
            }
            else
            {
                if (GlobalData.VM_MA.IsLiked_Document)
                    return;

                GlobalData.VM_MA.Messages = ret.Msgs;
            }
        });
    }
}
