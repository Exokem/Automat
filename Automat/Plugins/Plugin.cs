using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
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

        public virtual void Import(JObject data) { }

        public virtual JObject Export() => null;
    }

    public class DeviceGeneratorPlugin : Plugin
    {
        readonly TextInputField _deviceName, _deviceCategory;
        readonly DirectoryInputField _packageDir, _resourceRoot;
        readonly CheckInputField _setupFormat;

        public DeviceGeneratorPlugin() : base("Device Generator")
        {
            _deviceName = AddField(new TextInputField("Device Name"));
            _deviceCategory = AddField(new TextInputField("Device Category"));
            _packageDir = AddField(new DirectoryInputField("Package Directory"));
            _resourceRoot = AddField(new DirectoryInputField("Resource Root Directory"));
            _setupFormat = AddField(new CheckInputField("Format Setup"));
        }

        protected override void Execute()
        {
            if (string.IsNullOrEmpty(_deviceName.Value) || string.IsNullOrEmpty(_deviceCategory.Value) || string.IsNullOrEmpty(_packageDir.Value) || string.IsNullOrEmpty(_resourceRoot.Value))
                return;

            // This directory contains the device enumeration class and the package that will be created for the device being generated
            DirectoryInfo packageDir = new DirectoryInfo(_packageDir.Value);

            FieldValues data = new FieldValues
            {
                // AutoMachine
                DisplayName = _deviceName.Value,

                // Auto Machine -> auto_machine
                Identifier = _deviceName.Value.ToLower().Replace(' ', '_'),

                // Auto Machine -> AUTO_MACHINE
                UpperIdentifier = _deviceName.Value.ToUpper().Replace(' ', '_'),

                // Auto Machine -> AutoMachine
                ClassPrefix = _deviceName.Value.Replace(" ", ""),

                // Construction -> construction
                CategoryIdentifier = _deviceCategory.Value.ToLower().Replace(' ', '_'),

                // Construction -> ConstructionDevice
                CategoryClass = $"{_deviceCategory.Value.Replace(" ", "")}Device",
            };

            GenerateDeviceClasses(packageDir, data);
        }

        const string _categoryIdentifierKey = "{$CATEGORY_IDENTIFIER}";
        const string _categoryClassKey = "{$CATEGORY_CLASS}";
        const string _identifierKey = "{$IDENTIFIER}";
        const string _upperIdentifierKey = "{$UPPER_IDENTIFIER}";
        const string _classPrefixKey = "{$CLASS_PREFIX}";

        const string _deviceTemplate = "package xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$IDENTIFIER};\r\n\r\nimport net.minecraft.core.BlockPos;\r\nimport net.minecraft.world.level.block.state.BlockState;\r\nimport xkv.pluton.device.DeviceBlockEntity;\r\nimport xkv.voltaic.axiom.Voltaic;\r\nimport xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$CATEGORY_CLASS};\r\n\r\npublic class {$CLASS_PREFIX}Device extends DeviceBlockEntity\r\n{\r\n    public {$CLASS_PREFIX}Device(BlockPos position, BlockState state)\r\n    {\r\n        super({$CATEGORY_CLASS}.{$UPPER_IDENTIFIER}.type(), position, state);\r\n    }\r\n}";
        const string _screenTemplate = "package xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$IDENTIFIER};\r\n\r\nimport net.minecraft.network.chat.Component;\r\nimport net.minecraft.world.entity.player.Inventory;\r\nimport xkv.pluton.device.DeviceScreen;\r\nimport xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$CATEGORY_CLASS};\r\n\r\npublic class {$CLASS_PREFIX}Screen extends DeviceScreen<{$CLASS_PREFIX}Menu>\r\n{\r\n    public {$CLASS_PREFIX}Screen({$CLASS_PREFIX}Menu menu, Inventory inventory, Component title)\r\n    {\r\n        super(menu, inventory, title, {$CATEGORY_CLASS}.{$UPPER_IDENTIFIER}.identifier);\r\n    }\r\n}\r\n";
        const string _menuTemplate = "package xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$IDENTIFIER};\r\n\r\nimport net.minecraft.core.BlockPos;\r\nimport net.minecraft.world.entity.player.Inventory;\r\nimport net.minecraft.world.entity.player.Player;\r\nimport net.minecraft.world.inventory.MenuType;\r\nimport org.jetbrains.annotations.Nullable;\r\nimport xkv.pluton.device.DeviceMenu;\r\nimport xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$CATEGORY_CLASS};\r\n\r\npublic class {$CLASS_PREFIX}Menu extends DeviceMenu\r\n{\r\n    public {$CLASS_PREFIX}Menu(int _windowId, BlockPos _position, Inventory _inventory, Player _player)\r\n    {\r\n        super({$CATEGORY_CLASS}.{$UPPER_IDENTIFIER}.menu(), _windowId, _position, _inventory, _player);\r\n    }\r\n\r\n    @Override\r\n    protected void addDeviceSlots()\r\n    {\r\n\r\n    }\r\n}\r\n";

        class FieldValues
        {
            public string DisplayName { get; init; }
            public string Identifier { get; init; }
            public string UpperIdentifier { get; init; }
            public string CategoryIdentifier { get; init; }
            public string CategoryClass { get; init; }
            public string ClassPrefix { get; init; }
        }

        void GenerateDeviceClasses(DirectoryInfo packageDir, FieldValues data)
        {
            // 1. Create package to contain auto-generated classes

            DirectoryInfo devicePackage = packageDir.Directory(data.Identifier);

            if (!devicePackage.Exists)
                devicePackage.Create();

            // 2. Generate device, screen, and menu classes

            TryWrite(devicePackage.File($"{data.ClassPrefix}Device.java"), () => ReplaceFields(_deviceTemplate, data));
            TryWrite(devicePackage.File($"{data.ClassPrefix}Menu.java"), () => ReplaceFields(_menuTemplate, data));
            TryWrite(devicePackage.File($"{data.ClassPrefix}Screen.java"), () => ReplaceFields(_screenTemplate, data));
        }

        void TryWrite(FileInfo file, Func<string> data)
        {
            if (!file.Exists)
            {
                file.Create().Close();
                file.WriteString(data());
            }
        }

        string ReplaceFields(string template, FieldValues data)
        {
            return template
                .Replace(_categoryIdentifierKey, data.CategoryIdentifier)
                .Replace(_categoryClassKey, data.CategoryClass)
                .Replace(_identifierKey, data.Identifier)
                .Replace(_upperIdentifierKey, data.UpperIdentifier)
                .Replace(_classPrefixKey, data.ClassPrefix);
        }

        void GenerateDeviceResources(string displayName, string identifier)
        {
            // Inject locale entry
            string locale = $"\"block.voltaic.{identifier}\": \"{displayName}\",";



        }

        const string _lpd = "LastPackageDirectory";
        const string _lrr = "LastResourceRoot";

        public override JObject Export()
        {
            JObject data = new();

            if (!string.IsNullOrEmpty(_packageDir.Value))
                data.Add(_lpd, _packageDir.Value);

            if (!string.IsNullOrEmpty(_resourceRoot.Value))
                data.Add(_lrr, _resourceRoot.Value);

            return data;
        }

        public override void Import(JObject data)
        {
            if (data.ContainsKey(_lpd))
                _packageDir.Value = data.GetString(_lpd);

            if (data.ContainsKey(_lrr))
                _resourceRoot.Value = data.GetString(_lrr);
        }

        
    }
}
