using CoreAudio;
using CoreAudio.Undocumented;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BeyondAudio
{
    internal class Program
    {
        protected static string BinaryName = "BeyondATC";

        public static int Main(string[] args)
        {
            Console.WriteLine("Getting BATC Path from Registry ...");
            string appPath = GetPathFromRegistry();
            string exePath = $@"{appPath}\{BinaryName}.exe";
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]) || string.IsNullOrWhiteSpace(appPath) || !File.Exists(exePath))
                return -1;

            Console.WriteLine("Starting BATC ...");
            StartProcess(exePath, appPath);
            Thread.Sleep(500);

            Console.WriteLine($"Searching for Device '{args[0]}' ...");
            MMDevice targetDevice = FindDevice(args[0]);

            Console.WriteLine($"Getting PID for BATC ...");
            var processes = Process.GetProcessesByName("BeyondATC");

            if (targetDevice == null || processes == null || processes.Length < 1)
                return -2;

            Console.WriteLine($"Switching Audio Output ...");
            if (SetDefaultEndPoint(targetDevice, processes[0].Id))
                return 0;
            else
                return -3;
        }

        protected static bool SetDefaultEndPoint(MMDevice targetDevice, int processId)
        {
            try
            {
                var audioPolicyConfig = AudioPolicyConfigFactory.Create();

                audioPolicyConfig.SetPersistedDefaultAudioEndpoint(processId, DataFlow.Render, Role.Multimedia | Role.Console | Role.Communications, targetDevice);

                return true;
            }
            catch { }

            return false;
        }

        protected static MMDevice FindDevice(string deviceName)
        {
            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in devices)
            {
                try
                {
                    Console.WriteLine(string.Format("Device '{0}' - {1}", device.DeviceFriendlyName, device.ID));
                    if (device.DeviceFriendlyName.ToLower().Contains(deviceName.ToLower()))
                    {
                        return device;
                    }
                }
                catch { }
            }

            return null;
        }

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

        public static string GetPathFromRegistry()
        {
            try
            {
                return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{CF88F4D6-A60E-473D-A1D3-ABB5FE336EFA}_is1", "InstallLocation", null);
            }
            catch { return null; }
        }
    }
}
