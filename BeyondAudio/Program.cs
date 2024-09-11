using CoreAudio;
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
            string exePath = $@"{args[0]}\{BinaryName}.exe";
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[0]) || string.IsNullOrWhiteSpace(args[1]) || !File.Exists(exePath))
                return -1;

            StartProcess(exePath, args[0]);
            Thread.Sleep(500);

            MMDevice targetDevice = FindDevice(args[1]);
            var processes = Process.GetProcessesByName("BeyondATC");

            if (targetDevice == null || processes == null || processes.Length < 1)
                return -2;

            if (SetDefaultEndPoint(targetDevice.ID, processes[0].Id))
                return 0;
            else
                return -3;
        }

        protected static bool SetDefaultEndPoint(string deviceId, int processId)
        {
            try
            {
                var audioPolicyConfig = AudioPolicyConfigFactory.Create();

                IntPtr hstring = IntPtr.Zero;

                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    var str = GenerateDeviceId(deviceId);
                    Combase.WindowsCreateString(str, (uint)str.Length, out hstring);
                }

                audioPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, EDataFlow.eRender, ERole.eMultimedia, hstring);
                audioPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, EDataFlow.eRender, ERole.eConsole, hstring);
                audioPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, EDataFlow.eRender, ERole.eCommunications, hstring);

                return true;
            }
            catch { }

            return false;
        }

        protected static string GenerateDeviceId(string deviceId)
        {
            return $@"\\?\SWD#MMDEVAPI#{deviceId}#{{e6327cad-dcec-4949-ae8a-991e976a79d2}}";
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
    }
}
