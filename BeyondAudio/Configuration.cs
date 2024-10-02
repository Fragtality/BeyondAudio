using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAudio;

namespace BeyondAudio
{
    public class Configuration
    {
        [JsonIgnore]
        public static string REG_PATH { get; } = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{CF88F4D6-A60E-473D-A1D3-ABB5FE336EFA}_is1";

        [JsonIgnore]
        public static string REG_VALUE { get; } = "InstallLocation";

        [JsonIgnore]
        public static string APP_NAME { get; } = "BeyondAudio";

        [JsonIgnore]
        public static string APP_DIR { get; } = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{APP_NAME}";

        [JsonIgnore]
        public static string CFG_FILE { get; } = $@"{APP_DIR}\Device.json";

        public string DeviceName { get; set; } = "";
        public int AppDelay { get; set; } = 500;
        public Role Role { get; set; } = 0;

        public static Configuration Load()
        {
            if (File.Exists(CFG_FILE))
                return JsonSerializer.Deserialize<Configuration>(File.ReadAllText(CFG_FILE));
            else
                return null;
        }

        public void Save()
        {
            File.WriteAllText(CFG_FILE, JsonSerializer.Serialize(this, new JsonSerializerOptions() {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            }));
        }
    }
}
