using System;
using System.Globalization;
using System.IO;
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
        public static string Text_UseBrowser { get; private set; }

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
        public static string Msg_Logout { get; private set; }
        public static string Msg_NeedLogin { get; private set; }
        public static string Msg_ChangeAccount { get; private set; }

        #endregion

        private static string[] _cultureSupports = new string[] { "zh-CN", "ja-JP", "en-US" };

        private static string GetResource(string name)
        {
            var ln = Application.Current.TryFindResource(name);
            return ln == null ? string.Empty : (string)ln;
        }

        private static void Initialize()
        {
            Text_CookiesFanbox = GetResource("Text_CookiesTagFanbox");
            Text_CookiesFantia = GetResource("Text_CookiesTagFantia");
            Text_CookiesPatreon = GetResource("Text_CookiesTagPatreon");
            Text_CreatorUrlFanbox = GetResource("Text_CreatorUrlTagFanbox");
            Text_CreatorUrlFantia = GetResource("Text_CreatorUrlTagFantia");
            Text_CreatorUrlPatreon = GetResource("Text_CreatorUrlTagPatreon");
            Text_DefaultAName = GetResource("Text_DefaultName");
            Text_FilePref = GetResource("Text_FilePrefix");
            Text_ImagePref = GetResource("Text_ImagePrefix");
            Text_LinkPref = GetResource("Text_LinkPrefix");
            Text_AllStart = GetResource("Text_PlayButton");
            Text_AllPause = GetResource("Text_PauseButton");
            Text_FList = GetResource("Text_FileList");
            Text_IList = GetResource("Text_ImageList");
            Text_UnKnownSize = GetResource("Text_FileSize");
            Text_FileCou = GetResource("Text_FileCount");
            Text_FileImgCou = GetResource("Text_FICount");
            Text_NoCov = GetResource("Text_NoCover");
            Text_NeedLC = GetResource("Text_NeedLoad");
            Text_UseBrowser = GetResource("Text_Browser");

            Msg_ExitApp = GetResource("Msg_Exit");
            Msg_StartConf = GetResource("Msg_StartConfirm");
            Msg_SecondConf = GetResource("Msg_SecondConfirm");
            Msg_IsDownload = GetResource("Msg_IsDownloading");
            Msg_ChangeSP = GetResource("Msg_ChangeSavePath");
            Msg_CheckSP = GetResource("Msg_CheckSavePath");
            Msg_CreateSP = GetResource("Msg_CreateSavePathError");
            Msg_CheckCk = GetResource("Msg_CheckCookies");
            Msg_NoPosts = GetResource("Msg_NoPosts");
            Msg_ErrorCookies = GetResource("Msg_CookieError");
            Msg_ErrorCookiesAuto = GetResource("Msg_CookieErrorAuto");
            Msg_ErrorCookiesMail = GetResource("Msg_CookieErrorMail");
            Msg_ErrorIO = GetResource("Msg_IOError");
            Msg_ErrorPath = GetResource("Msg_PathError");
            Msg_ErrorSecurity = GetResource("Msg_SecurityError");
            Msg_ErrorUnKnown = GetResource("Msg_UnKnownError");
            Msg_ErrorUnSupported = GetResource("Msg_UnKnownErrorUnSupported");
            Msg_ErrorWeb = GetResource("Msg_WebError");
            Msg_ErrorWebProxy = GetResource("Msg_WebErrorProxy");
            Msg_DLErrWeb = GetResource("Msg_DLErrorWeb");
            Msg_DLErrPath = GetResource("Msg_DLErrorPath");
            Msg_DLErrSecu = GetResource("Msg_DLErrorSecurity");
            Msg_LoadCov = GetResource("Msg_LoadCover");
            Msg_CoverErr = GetResource("Msg_CoverError");
            Msg_Logout = GetResource("Msg_LogOutMsg");
            Msg_NeedLogin = GetResource("Msg_NeedLoginMsg");
            Msg_ChangeAccount = GetResource("Msg_ChangeAccountMsg");
        }

        public static void InitializeCulture()
        {
            try
            {
                if (int.TryParse(File.ReadAllText("Lang.ini"), out int temp))
                {
                    SetCulture(temp);
                    GlobalData.CurrentCulStr = temp;
                    return;
                }
            }
            catch { }
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
            try
            {
                File.WriteAllText("Lang.ini", index.ToString());
            }
            catch { }
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
