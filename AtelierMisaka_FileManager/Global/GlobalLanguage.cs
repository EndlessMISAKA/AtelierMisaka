using System;
using System.Globalization;
using System.IO;
using System.Windows;

namespace AtelierMisaka_FileManager
{
    public class GlobalLanguage
    {
        #region Fields_Message
        public static string Msg_ErrorIO { get; private set; }
        public static string Msg_ErrorPath { get; private set; }

        public static string Msg_ExitApp { get; private set; }
        public static string Msg_NoPosts { get; private set; }

        #endregion

        private static string[] _cultureSupports = new string[] { "zh-CN", "ja-JP", "en-US" };
        
        private static void Initialize()
        {
            Msg_ErrorIO = Application.Current.TryFindResource("Msg_IOError").ToString();
            Msg_ErrorPath = Application.Current.TryFindResource("Msg_PathError").ToString();
            Msg_ExitApp = Application.Current.TryFindResource("Msg_Exit").ToString();
        }

        public static void InitializeCulture()
        {
            if (File.Exists("Lang.ini"))
            {
                if (int.TryParse(File.ReadAllText("Lang.ini"), out int temp))
                {
                    SetCulture(temp);
                    GlobalData.CurrentCulStr = temp;
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
        }
    }
}
