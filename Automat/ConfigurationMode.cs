using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

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

        public static void Execute(string key, DirectoryInfo source, DirectoryInfo destination)
        {
            if (key == null)
                return;

            IConfigurationMode mode = Get(key);

            if (mode == null)
                return;

            if (mode.Scan(source))
                mode.Apply(destination);
        }

        string Key();

        bool Scan(DirectoryInfo source);

        void Apply(DirectoryInfo destination);
    }

    public class AsepriteConfiguration : IConfigurationMode
    {
        public AsepriteConfiguration() { }

        public string Key() => "Aseprite";

        public bool Scan(DirectoryInfo source)
        {
            throw new NotImplementedException();
        }

        public void Apply(DirectoryInfo destination)
        {
            throw new NotImplementedException();
        }
    }
}
