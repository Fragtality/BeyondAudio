using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows;

namespace BeyondAudio
{
    public static class Tools
    {
        public static void StartProcess(string absolutePath, string workDirectory = null, string args = null)
        {
            var pProcess = new Process();
            pProcess.StartInfo.FileName = absolutePath;
            pProcess.StartInfo.UseShellExecute = true;
            pProcess.StartInfo.WorkingDirectory = workDirectory ?? Directory.GetCurrentDirectory();
            if (args != null)
                pProcess.StartInfo.Arguments = args;
            pProcess.Start();
        }

        public static string GetValueFromRegistry(string path, string value)
        {
            try
            {
                return (string)Registry.GetValue(path, value, null);
            }
            catch { return null; }
        }

        public static string GetBatcDirectory()
        {
            return GetValueFromRegistry(Configuration.REG_PATH, Configuration.REG_VALUE);
        }

        public static string GetBatcBinary()
        {
            return $@"{GetValueFromRegistry(Configuration.REG_PATH, Configuration.REG_VALUE)}\BeyondATC.exe";
        }

        public static bool PlaceDesktopLink()
        {
            bool result = false;
            try
            {
                IShellLink link = (IShellLink)new ShellLink();

                link.SetDescription("Start BeyondATC through BeyondAudio");
                link.SetPath($@"{Configuration.APP_DIR}\{Configuration.APP_NAME}.exe");
                link.SetWorkingDirectory(Configuration.APP_DIR);
                link.SetIconLocation(GetBatcBinary(), 0);

                IPersistFile file = (IPersistFile)link;
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                file.Save(Path.Combine(desktopPath, "BeyondATC (BeyondAudio).lnk"), false);
                result = true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception '{e.GetType()}' during PlaceDesktopLink", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return result;
        }
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}
