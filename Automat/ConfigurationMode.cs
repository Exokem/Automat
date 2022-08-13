using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Automat
{
    public interface IConfigurationMode
    {
        private static readonly Dictionary<string, IConfigurationMode> _registeredModes;

        static IConfigurationMode()
        {
            _registeredModes = new Dictionary<string, IConfigurationMode>();
            Register<AsepriteConfiguration>();
        }

        public static void Register<V>()
        {
            try
            {
                IConfigurationMode mode = Activator.CreateInstance(typeof(V)) as IConfigurationMode;

                if (mode != null)
                {
                    _registeredModes[mode.Key()] = mode;
                }
            }

            catch (Exception ex)
            {
                
            }
        }

        private static IConfigurationMode Get(string key)
        {
            if (!_registeredModes.ContainsKey(key))
                return null;

            if (_registeredModes[key] == null)
            {
                _registeredModes.Remove(key);
                return null;
            }

            return _registeredModes[key];
        }

        public static void Execute(string key, DirectoryInfo source, DirectoryInfo destination, Func<string, bool> fileFilter)
        {
            if (key == null)
                return;

            IConfigurationMode mode = Get(key);

            if (mode == null)
                return;

            if (mode.Scan(source, fileFilter))
                mode.Apply(destination);
        }

        internal static void WriteExtraData(JObject data)
        {
            foreach ((string key, IConfigurationMode mode) in _registeredModes)
            {
                JObject ex = new JObject();
                mode.Write(ex);
                data[key] = ex;
            }
        }

        internal static void ReadExtraData(JObject data)
        {
            foreach ((string key, IConfigurationMode mode) in _registeredModes)
            {
                if (data.ContainsKey(key) && data[key] is JObject ex)
                    mode.Read(ex);
            }
        }

        string Key();

        bool Scan(DirectoryInfo source, Func<string, bool> fileFilter);

        void Apply(DirectoryInfo destination);

        void Write(JObject data);

        void Read(JObject data);
    }

    public class AsepriteConfiguration : IConfigurationMode
    {
        public AsepriteConfiguration() { }

        public string Key() => "Aseprite";

        string AsepritePath;

        string CommandString(string file, string destination) => $"@start \"\" \"{AsepritePath}\" -b \"{file}\" --save-as \"{destination}\\{{slice}}.png\"";

        readonly Queue<FileInfo> _files = new Queue<FileInfo>();

        public bool Scan(DirectoryInfo source, Func<string, bool> fileFilter)
        {
            if (AsepritePath == null)
                AsepritePath = FileDialogUtility.SelectFile("Select Aseprite Executable", from: Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

            if (AsepritePath == null)
                return false;

            foreach (FileInfo file in source.EnumerateFiles().Where(file => fileFilter(file.FullName)))
                _files.Enqueue(file);

            return true;
        }

        public void Apply(DirectoryInfo destination)
        {
            ProcessStartInfo cmdInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = "@echo off",
                CreateNoWindow = false,
                RedirectStandardInput = true,
                UseShellExecute = false,
            };
            Process cmd = new Process();
            cmd.StartInfo = cmdInfo;
            cmd.Start();

            while (_files.Count != 0)
            {
                FileInfo file = _files.Dequeue();
                cmd.StandardInput.WriteLine(CommandString(file.FullName, destination.FullName));
            }
        }

        public void Write(JObject data)
        {
            data[nameof(AsepritePath)] = AsepritePath;
        }

        public void Read(JObject data)
        {
            AsepritePath = (string)data[nameof(AsepritePath)];
        }
    }
}
