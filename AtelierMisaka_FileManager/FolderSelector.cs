﻿using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AtelierMisaka_FileManager
{
    #region Editor

    public class FolderNameEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            FolderSelector browser = new FolderSelector();
            if (value != null)
            {
                browser.DirectoryPath = string.Format("{0}", value);
            }
            if (browser.ShowDialog(null) == DialogResult.OK)
                return browser.DirectoryPath;
            return value;
        }
    }
    #endregion

    #region FolderBrowserDialog Base

    [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
    public class FolderSelector : Component
    {
        public FolderSelector() : this(null) { }
        public FolderSelector(string dir)
        {
            DirectoryPath = dir;
        }

        #region Public Property

        public string DirectoryPath { get; set; }

        public DialogResult ShowDialog(IWin32Window owner)
        {
            IntPtr hwndOwner = owner != null ? owner.Handle : GetActiveWindow();
            IFileOpenDialog dialog = (IFileOpenDialog)new FileOpenDialog();
            try
            {
                IShellItem item;
                if (!string.IsNullOrEmpty(DirectoryPath))
                {
                    IntPtr idl;
                    uint atts = 0;
                    if (SHILCreateFromPath(DirectoryPath, out idl, ref atts) == 0)
                    {
                        if (SHCreateShellItem(IntPtr.Zero, IntPtr.Zero, idl, out item) == 0)
                        {
                            dialog.SetFolder(item);
                        }
                    }
                }
                dialog.SetOptions(FOS.FOS_PICKFOLDERS | FOS.FOS_FORCEFILESYSTEM);
                uint hr = dialog.Show(hwndOwner);
                if (hr == ERROR_CANCELLED)
                    return DialogResult.Cancel;

                if (hr != 0)
                    return DialogResult.Abort;
                dialog.GetResult(out item);
                string path;
                item.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out path);
                DirectoryPath = path;
                return DialogResult.OK;
            }
            finally
            {
                Marshal.ReleaseComObject(dialog);
            }
        }
        #endregion

        #region BaseType
        [DllImport("shell32.dll")]
        private static extern int SHILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath, out IntPtr ppIdl, ref uint rgflnOut);
        [DllImport("shell32.dll")]
        private static extern int SHCreateShellItem(IntPtr pidlParent, IntPtr psfParent, IntPtr pidl, out IShellItem ppsi);
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        private const uint ERROR_CANCELLED = 0x800704C7;
        [ComImport]
        [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class FileOpenDialog
        {
        }
        [ComImport]
        [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig]
            uint Show([In] IntPtr parent); // IModalWindow
            void SetFileTypes();  // not fully defined
            void SetFileTypeIndex([In] uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise(); // not fully defined
            void Unadvise();
            void SetOptions([In] FOS fos);
            void GetOptions(out FOS pfos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int alignment);
            void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid();  // not fully defined
            void ClearClientData();
            void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
            void GetResults([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenum); // not fully defined
            void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppsai); // not fully defined
        }
        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(); // not fully defined
            void GetParent(); // not fully defined
            void GetDisplayName([In] SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes();  // not fully defined
            void Compare();  // not fully defined
        }
        private enum SIGDN : uint
        {
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
            SIGDN_FILESYSPATH = 0x80058000,
            SIGDN_NORMALDISPLAY = 0,
            SIGDN_PARENTRELATIVE = 0x80080001,
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,
            SIGDN_URL = 0x80068000
        }
        [Flags]
        private enum FOS
        {
            FOS_ALLNONSTORAGEITEMS = 0x80,
            FOS_ALLOWMULTISELECT = 0x200,
            FOS_CREATEPROMPT = 0x2000,
            FOS_DEFAULTNOMINIMODE = 0x20000000,
            FOS_DONTADDTORECENT = 0x2000000,
            FOS_FILEMUSTEXIST = 0x1000,
            FOS_FORCEFILESYSTEM = 0x40,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_HIDEMRUPLACES = 0x20000,
            FOS_HIDEPINNEDPLACES = 0x40000,
            FOS_NOCHANGEDIR = 8,
            FOS_NODEREFERENCELINKS = 0x100000,
            FOS_NOREADONLYRETURN = 0x8000,
            FOS_NOTESTFILECREATE = 0x10000,
            FOS_NOVALIDATE = 0x100,
            FOS_OVERWRITEPROMPT = 2,
            FOS_PATHMUSTEXIST = 0x800,
            FOS_PICKFOLDERS = 0x20,
            FOS_SHAREAWARE = 0x4000,
            FOS_STRICTFILETYPES = 4
        }
        #endregion
    }
    #endregion

}
