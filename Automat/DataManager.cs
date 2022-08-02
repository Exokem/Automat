using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Automat
{
    internal static class DataManager
    {
        static readonly List<Configuration> _configurations = new List<Configuration>();

        static readonly DirectoryInfo _appData = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        static readonly DirectoryInfo _saveDir = _appData.Directory("Automat");
        static readonly FileInfo _saveFile = _saveDir.File("UserData.json");

        internal static void SaveConfiguration(Configuration cfg)
        {
            _configurations.Add(cfg);
            Save();
        }

        static void Validate()
        {
            if (!_appData.Exists)
                throw new InvalidOperationException("Unable to locate user app data");

            if (!_saveDir.Verify())
                throw new InvalidOperationException("Unable to create missing data directory");
        }

        internal static void Save()
        {
            Validate();

            JObject modeData = new JObject();

            IConfigurationMode.WriteExtraData(modeData);

            JObject configurationData = new JObject();

            JArray configurations = new JArray();

            _configurations.ForEach(cfg => configurations.Add(cfg.Export()));

            configurationData["Configurations"] = configurations;
            configurationData["ModeData"] = modeData;

            _saveFile.WriteString(configurationData.ToString());
        }

        internal static void LoadConfigurations(Action<Configuration> configurationReceiver)
        {
            Validate();

            JObject configurationData = JObject.Parse(_saveFile.ReadString());

            try
            {
                JArray configurations = configurationData["Configurations"] as JArray;

                foreach (JObject configuration in configurations)
                {
                    try
                    {
                        Configuration cfg = new Configuration(configuration);
                        _configurations.Add(cfg);
                        configurationReceiver(cfg);
                    }

                    catch (Exception e) 
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                }

                JObject modeData = configurationData["ModeData"] as JObject;

                if (modeData != null)
                    IConfigurationMode.ReadExtraData(modeData);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

        }
    }

    public static class DirectoryInfoExtensions
    {
        public static void WriteData(FileInfo file, string data)
        {
            try
            {
                System.IO.File.WriteAllText(file.FullName, data);
            }

            catch (Exception e)
            {
                Console.WriteLine($"Write operation failed: {e.Message}");
                Console.WriteLine(e.StackTrace);
            }
        }

        public static string ReadData(FileInfo file)
        {
            try
            {
                return System.IO.File.ReadAllText(file.FullName);
            }

            catch (Exception e)
            {
                Console.WriteLine($"Read operation failed: {e.Message}");
                Console.WriteLine(e.StackTrace);

                return "";
            }
        }

        public static FileInfo File(this DirectoryInfo container, string file)
        {
            return new FileInfo($"{container.FullName}\\{file}");
        }

        public static DirectoryInfo Directory(this DirectoryInfo container, string directory)
        {
            return new DirectoryInfo($"{container.FullName}\\{directory}");
        }

        public static void Verify(this FileInfo file, string defaultContent = null)
        {
            if (!file.Exists)
            {
                file.Create();
                if (defaultContent != null)
                    file.WriteString(defaultContent);
            }
        }

        public static bool Verify(this DirectoryInfo directory)
        {
            if (!directory.Exists)
                directory.Create();

            return directory.Exists;
        }

        public static void WriteString(this FileInfo file, string data)
        {
            WriteData(file, data);
        }

        public static string ReadString(this FileInfo file)
        {
            return ReadData(file);
        }
    }

    public static class JsonExtensions
    {
        public static JArray ToJArray<V>(this IEnumerable<V> container)
        {
            var jArray = new JArray();
            foreach (var item in container)
                jArray.Add(item);
            return jArray;
        }

        public static V ToCollection<V>(this JArray array, V collection) where V : ICollection<string>
        {
            foreach (var item in array)
                collection.Add((string)item);

            return collection;
        }

        public static string GetString(this JObject data, string key, string def = null)
        {
            if (data.ContainsKey(key))
                return (string)data[key];
            return def;
        }

        public static int GetInt(this JObject data, string key, int def = 0)
        {
            if (data.ContainsKey(key))
                return (int)data[key];
            return def;
        }

        public static bool GetBool(this JObject data, string key, bool def = false)
        {
            if (data.ContainsKey(key))
                return (bool)data[key];
            return def;
        }
    }
}
