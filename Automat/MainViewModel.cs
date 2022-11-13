using Automat.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Automat
{
    public class MainViewModel : Notifier
    {
        public const int ConfigurationsMode = 0;
        public const int PluginsMode = 1;

        public ICommand PanelModeCommand { get; }

        Dictionary<int, string> _modes = new()
        {
            { ConfigurationsMode, "Configurations" },
            { PluginsMode, "Plugins" },
        };

        readonly ImmutableList<Plugin> _plugins;
        public IEnumerable<Plugin> Plugins => _plugins;

        int _mode = ConfigurationsMode;
        public int Mode
        {
            get => _mode;
            set
            {
                Notify(ref _mode, value);
                Notify(nameof(ShowConfigurationsPanel));
                Notify(nameof(ShowPluginsPanel));

                ModeName = _modes[value];
                Notify(nameof(ModeName));
            }
        }

        public string ModeName { get; private set; } = "Configurations";

        public bool ShowConfigurationsPanel => Mode == ConfigurationsMode;

        public bool ShowPluginsPanel => Mode == PluginsMode;

        public MainViewModel()
        {
            PanelModeCommand = new Command(SetPanelMode);

            _plugins = new List<Plugin>
            {
                new DeviceGeneratorPlugin(),

            }.ToImmutableList();
        }

        void SetPanelMode(object o)
        {
            if (o is int mode && _modes.ContainsKey(mode))
            {
                Mode = mode;

            }
        }
    }
}
