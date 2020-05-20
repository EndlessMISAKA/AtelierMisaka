using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AtelierMisaka
{
    public class GlobalLanguage
    {
        #region Fields_Text

        public static string Text_CookiesFanbox { get; private set; }
        public static string Text_CookiesFantia { get; private set; }
        public static string Text_CookiesPatreon { get; private set; }
        public static string Text_CreatorUrlFanbox { get; private set; }
        public static string Text_CreatorUrlFantia { get; private set; }
        public static string Text_CreatorUrlPatreon { get; private set; }
        public static string Text_DefaultAName { get; private set; }
        public static string Text_FilePref { get; private set; }
        public static string Text_ImagePref { get; private set; }
        public static string Text_LinkPref { get; private set; }
        public static string Text_AllStart { get; private set; }
        public static string Text_AllPause { get; private set; }
        public static string Text_FList { get; private set; }
        public static string Text_IList { get; private set; }
        public static string Text_UnKnownSize { get; private set; }
        public static string Text_FileCou { get; private set; }
        public static string Text_FileImgCou { get; private set; }
        public static string Text_NoCov { get; private set; }
        public static string Text_NeedLC { get; private set; }

        #endregion

        #region Fields_Message

        public static string Msg_ErrorWeb { get; private set; }
        public static string Msg_ErrorWebProxy { get; private set; }
        public static string Msg_ErrorCookies { get; private set; }
        public static string Msg_ErrorCookiesMail { get; private set; }
        public static string Msg_ErrorCookiesAuto { get; private set; }
        public static string Msg_ErrorIO { get; private set; }
        public static string Msg_ErrorPath { get; private set; }
        public static string Msg_ErrorSecurity { get; private set; }
        public static string Msg_ErrorUnKnown { get; private set; }
        public static string Msg_ErrorUnSupported { get; private set; }

        public static string Msg_ExitApp { get; private set; }
        public static string Msg_StartConf { get; private set; }
        public static string Msg_SecondConf { get; private set; }
        public static string Msg_IsDownload { get; private set; }
        public static string Msg_ChangeSP { get; private set; }
        public static string Msg_CheckSP { get; private set; }
        public static string Msg_CreateSP { get; private set; }
        public static string Msg_CheckCk { get; private set; }
        public static string Msg_NoPosts { get; private set; }
        public static string Msg_DLErrWeb { get; private set; }
        public static string Msg_DLErrPath { get; private set; }
        public static string Msg_DLErrSecu { get; private set; }
        public static string Msg_LoadCov { get; private set; }
        public static string Msg_CoverErr { get; private set; }

        #endregion

        private static string[] _cultureSupports = new string[] { "zh-CN", "ja-JP", "en-US" };
        
        private static void Initialize()
        {
            Text_CookiesFanbox = Application.Current.TryFindResource("Text_CookiesTagFanbox").ToString();
            Text_CookiesFantia = Application.Current.TryFindResource("Text_CookiesTagFantia").ToString();
            Text_CookiesPatreon = Application.Current.TryFindResource("Text_CookiesTagPatreon").ToString();
            Text_CreatorUrlFanbox = Application.Current.TryFindResource("Text_CreatorUrlTagFanbox").ToString();
            Text_CreatorUrlFantia = Application.Current.TryFindResource("Text_CreatorUrlTagFantia").ToString();
            Text_CreatorUrlPatreon = Application.Current.TryFindResource("Text_CreatorUrlTagPatreon").ToString();
            Text_DefaultAName = Application.Current.TryFindResource("Text_DefaultName").ToString();
            Text_FilePref = Application.Current.TryFindResource("Text_FilePrefix").ToString();
            Text_ImagePref = Application.Current.TryFindResource("Text_ImagePrefix").ToString();
            Text_LinkPref = Application.Current.TryFindResource("Text_LinkPrefix").ToString();
            Text_AllStart = Application.Current.TryFindResource("Text_PlayButton").ToString();
            Text_AllPause = Application.Current.TryFindResource("Text_PauseButton").ToString();
            Text_FList = Application.Current.TryFindResource("Text_FileList").ToString();
            Text_IList = Application.Current.TryFindResource("Text_ImageList").ToString();
            Text_UnKnownSize = Application.Current.TryFindResource("Text_FileSize").ToString();
            Text_FileCou = Application.Current.TryFindResource("Text_FileCount").ToString();
            Text_FileImgCou = Application.Current.TryFindResource("Text_FICount").ToString();
            Text_NoCov = Application.Current.TryFindResource("Text_NoCover").ToString();
            Text_NeedLC = Application.Current.TryFindResource("Text_NeedLoad").ToString();

            Msg_ExitApp = Application.Current.TryFindResource("Msg_Exit").ToString();
            Msg_StartConf = Application.Current.TryFindResource("Msg_StartConfirm").ToString();
            Msg_SecondConf = Application.Current.TryFindResource("Msg_SecondConfirm").ToString();
            Msg_IsDownload = Application.Current.TryFindResource("Msg_IsDownloading").ToString();
            Msg_ChangeSP = Application.Current.TryFindResource("Msg_ChangeSavePath").ToString();
            Msg_CheckSP = Application.Current.TryFindResource("Msg_CheckSavePath").ToString();
            Msg_CreateSP = Application.Current.TryFindResource("Msg_CreateSavePathError").ToString();
            Msg_CheckCk = Application.Current.TryFindResource("Msg_CheckCookies").ToString();
            Msg_NoPosts = Application.Current.TryFindResource("Msg_NoPosts").ToString();
            Msg_ErrorCookies = Application.Current.TryFindResource("Msg_CookieError").ToString();
            Msg_ErrorCookiesAuto = Application.Current.TryFindResource("Msg_CookieErrorMail").ToString();
            Msg_ErrorCookiesMail = Application.Current.TryFindResource("Msg_CookieErrorAuto").ToString();
            Msg_ErrorIO = Application.Current.TryFindResource("Msg_IOError").ToString();
            Msg_ErrorPath = Application.Current.TryFindResource("Msg_PathError").ToString();
            Msg_ErrorSecurity = Application.Current.TryFindResource("Msg_SecurityError").ToString();
            Msg_ErrorUnKnown = Application.Current.TryFindResource("Msg_UnKnownError").ToString();
            Msg_ErrorUnSupported = Application.Current.TryFindResource("Msg_UnKnownErrorUnSupported").ToString();
            Msg_ErrorWeb = Application.Current.TryFindResource("Msg_WebError").ToString();
            Msg_ErrorWebProxy = Application.Current.TryFindResource("Msg_WebErrorProxy").ToString();
            Msg_DLErrWeb = Application.Current.TryFindResource("Msg_DLErrorWeb").ToString();
            Msg_DLErrPath = Application.Current.TryFindResource("Msg_DLErrorPath").ToString();
            Msg_DLErrSecu = Application.Current.TryFindResource("Msg_DLErrorSecurity").ToString();
            Msg_LoadCov = Application.Current.TryFindResource("Msg_LoadCover").ToString();
            Msg_CoverErr = Application.Current.TryFindResource("Msg_CoverError").ToString();
        }

        public static void InitializeCulture()
        {
            if (File.Exists("Lang.ini"))
            {
                if (int.TryParse(File.ReadAllText("Lang.ini"), out int temp))
                {
                    SetCulture(temp);
                    return;
                }
            }
            SetCulture(CultureInfo.CurrentCulture.Name);
        }

        public static void SetCulture(int index)
        {
            ChangeCulture(_cultureSupports[index]);
            SaveConfig(index);
        }

        public static void SetCulture(string cultureName)
        {
            int index = Array.IndexOf(_cultureSupports, cultureName);
            if (index == -1)
            {
                cultureName = "en-US";
                GlobalData.CurrentCulStr = 2;
            }
            else
            {
                GlobalData.CurrentCulStr = index;
            }
            ChangeCulture(cultureName);
            SaveConfig(index);
        }

        private static void SaveConfig(int index)
        {
            File.WriteAllText("Lang.ini", index.ToString());
        }

        private static void ChangeCulture(string cultureName)
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri($"Styles/Style.{cultureName}.xaml", UriKind.Relative);
            if (Application.Current.Resources.MergedDictionaries.Count > 2)
            {
                Application.Current.Resources.MergedDictionaries.RemoveAt(2);
            }
            Application.Current.Resources.MergedDictionaries.Add(dict);
            Initialize();
            GlobalMethord.UpdateCulture();
        }
    }
}
