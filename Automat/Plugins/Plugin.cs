using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Automat.Plugins
{
    public abstract class Plugin
    {
        public string Name { get; }

        public PluginView PluginView { get; }

        readonly List<Field> _fields = new();
        public IEnumerable<Field> Fields => _fields;

        public ICommand ExecuteCommand { get; }

        protected Plugin(string name)
        {
            Name = name;
            PluginView = new PluginView { DataContext = this };
            ExecuteCommand = new Command(Execute);
        }

        protected F AddField<F>(F field) where F : Field
        {
            _fields.Add(field);
            return field;
        }

        protected abstract void Execute();
    }

    public class DeviceGeneratorPlugin : Plugin
    {
        readonly TextInputField _deviceName, _deviceEnum;
        readonly DirectoryInputField _packageDir;
        readonly CheckInputField _setupFormat;

        public DeviceGeneratorPlugin() : base("Device Generator")
        {
            _deviceName = AddField(new TextInputField("Device Name"));
            _deviceEnum = AddField(new TextInputField("Device Enum"));
            _packageDir = AddField(new DirectoryInputField("Package Directory"));
            _setupFormat = AddField(new CheckInputField("Format Setup"));
        }

        protected override void Execute()
        {
            
        }
    }
}
