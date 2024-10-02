using CoreAudio;
using CoreAudio.Undocumented;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace BeyondAudio
{
    public partial class App : Application
    {
        public static List<MMDevice> DeviceEnumeration { get; } = new List<MMDevice>();
        public static Configuration AppConfiguration { get; private set; } = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                EnumerateDevices();

                if (Installer())
                {
                    Shutdown(0);
                    return;
                }
                else
                    AppConfiguration = Configuration.Load();

                if (AppConfiguration == null)
                {
                    MessageBox.Show("Configuration could not be loaded!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown(-1);
                    return;
                }

                StartSetBatc();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception '{ex.GetType()}' during OnStartup", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-99);
                return;
            }
        }

        private void StartSetBatc()
        {
            Console.WriteLine("Finding Device ...");
            var targetDevice = FindDevice();
            if (targetDevice == null)
            {
                MessageBox.Show($"The Device '{AppConfiguration.DeviceName}' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-2);
                return;
            }

            Console.WriteLine("Starting BATC ...");
            Tools.StartProcess(Tools.GetBatcBinary(), Tools.GetBatcDirectory());
            Thread.Sleep(AppConfiguration.AppDelay);

            Console.WriteLine($"Getting PID for BATC ...");
            var processes = Process.GetProcessesByName("BeyondATC");

            if (processes == null || processes.Length < 1)
            {
                MessageBox.Show($"Could not find PID for BeyondATC!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-3);
                return;
            }

            Console.WriteLine($"Switching Audio Output ...");
            try
            {
                var audioPolicyConfig = AudioPolicyConfigFactory.Create();
                audioPolicyConfig.SetPersistedDefaultAudioEndpoint(processes[0].Id, DataFlow.Render, AppConfiguration.Role, targetDevice);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception '{ex.GetType()}' during StartSetBatc", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-4);
                return;
            }

            Shutdown(0);
        }

        private void EnumerateDevices()
        {
            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in devices)
            {
                try
                {
                    Console.WriteLine(string.Format("Enumerating Device '{0}' - {1}", device.DeviceFriendlyName, device.ID));
                    if (!string.IsNullOrWhiteSpace(device.DeviceFriendlyName))
                    {
                        device.AudioSessionManager2.RefreshSessions();
                        DeviceEnumeration.Add(device);
                    }
                }
                catch { }
            }
        }

        private MMDevice FindDevice()
        {
            foreach (var device in DeviceEnumeration)
            {
                if (device.DeviceFriendlyName.Contains(AppConfiguration.DeviceName))
                {
                    return device;
                }
            }

            return null;
        }

        private bool Installer()
        {
            if (Directory.GetCurrentDirectory() != Configuration.APP_DIR || !File.Exists(Configuration.CFG_FILE))
            {
                Console.WriteLine("Running in Configuration Mode");

                if (!Directory.Exists(Configuration.APP_DIR))
                {
                    Directory.CreateDirectory(Configuration.APP_DIR);
                    Console.WriteLine("Application Directory created");
                }

                if (Directory.GetCurrentDirectory() != Configuration.APP_DIR)
                {
                    File.Copy($"{Configuration.APP_NAME}.exe", $@"{Configuration.APP_DIR}\{Configuration.APP_NAME}.exe", true);
                    Console.WriteLine("Application Binary copied");
                }

                if (File.Exists(Configuration.CFG_FILE))
                {
                    File.Delete(Configuration.CFG_FILE);
                    Console.WriteLine("Application Configuration deleted");
                }

                DeviceSelection window = new DeviceSelection();
                if (window.ShowDialog() != true)
                {
                    Console.WriteLine("Configuration canceled - Exiting ...");
                }
                else if (window.StartBatc)
                {
                    Tools.StartProcess($@"{Configuration.APP_DIR}\{Configuration.APP_NAME}.exe", Configuration.APP_DIR);
                }

                return true;
            }

            return false;
        }
    }
}
