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
        sealed class FieldValues
        {
            public string DisplayName { get; init; }
            public string Identifier { get; init; }
            public string UpperIdentifier { get; init; }
            public string CategoryIdentifier { get; init; }
            public string CategoryClass { get; init; }
            public string ClassPrefix { get; init; }
        }

        static void TryWrite(FileInfo file, Func<string> data)
        {
            if (!file.Exists)
            {
                file.Create().Close();
                file.WriteString(data());
            }
        }

        static string ReplaceFields(string template, FieldValues data)
        {
            return template
                .Replace(_categoryIdentifierKey, data.CategoryIdentifier)
                .Replace(_categoryClassKey, data.CategoryClass)
                .Replace(_identifierKey, data.Identifier)
                .Replace(_upperIdentifierKey, data.UpperIdentifier)
                .Replace(_classPrefixKey, data.ClassPrefix);
        }

        readonly TextInputField _deviceName, _deviceCategory;
        readonly DirectoryInputField _sourceDir, _resourceRoot;
        readonly CheckInputField _setupFormat;

        public DeviceGeneratorPlugin() : base("Device Generator")
        {
            _deviceName = AddField(new TextInputField("Device Name"));
            _deviceCategory = AddField(new TextInputField("Device Category"));
            _sourceDir = AddField(new DirectoryInputField("Source Root Directory"));
            _resourceRoot = AddField(new DirectoryInputField("Resource Root Directory"));
            _setupFormat = AddField(new CheckInputField("Format Setup"));
        }

        protected override void Execute()
        {
            if (string.IsNullOrEmpty(_deviceName.Value) || string.IsNullOrEmpty(_deviceCategory.Value) || string.IsNullOrEmpty(_sourceDir.Value) || string.IsNullOrEmpty(_resourceRoot.Value))
                return;

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

            // xkv.voltaic
            DirectoryInfo sourceDir = new DirectoryInfo(_sourceDir.Value);

            // This directory contains the device enumeration class and the package that will be created for the device being generated
            // xkv.voltaic.devices.CATEGORY
            DirectoryInfo packageDir = sourceDir.Directory($"device\\{data.CategoryIdentifier}");

            DirectoryInfo resourceDir = new DirectoryInfo(_resourceRoot.Value);

            GenerateDeviceClasses(packageDir, data);

            GenerateDeviceResources(resourceDir, data);

            if (_setupFormat.Value)
                GenerateFormatSetup(sourceDir, resourceDir, data);
        }

        const string _categoryIdentifierKey = "{$CATEGORY_IDENTIFIER}";
        const string _categoryClassKey = "{$CATEGORY_CLASS}";
        const string _identifierKey = "{$IDENTIFIER}";
        const string _upperIdentifierKey = "{$UPPER_IDENTIFIER}";
        const string _classPrefixKey = "{$CLASS_PREFIX}";

        #region Code Generation

        const string _deviceTemplate = "package xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$IDENTIFIER};\r\n\r\nimport net.minecraft.core.BlockPos;\r\nimport net.minecraft.world.level.block.state.BlockState;\r\nimport xkv.pluton.device.DeviceBlockEntity;\r\nimport xkv.voltaic.axiom.Voltaic;\r\nimport xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$CATEGORY_CLASS};\r\n\r\npublic class {$CLASS_PREFIX}Device extends DeviceBlockEntity\r\n{\r\n    public {$CLASS_PREFIX}Device(BlockPos position, BlockState state)\r\n    {\r\n        super({$CATEGORY_CLASS}.{$UPPER_IDENTIFIER}.type(), position, state);\r\n    }\r\n}";
        const string _screenTemplate = "package xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$IDENTIFIER};\r\n\r\nimport net.minecraft.network.chat.Component;\r\nimport net.minecraft.world.entity.player.Inventory;\r\nimport xkv.pluton.device.DeviceScreen;\r\nimport xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$CATEGORY_CLASS};\r\n\r\npublic class {$CLASS_PREFIX}Screen extends DeviceScreen<{$CLASS_PREFIX}Menu>\r\n{\r\n    public {$CLASS_PREFIX}Screen({$CLASS_PREFIX}Menu menu, Inventory inventory, Component title)\r\n    {\r\n        super(menu, inventory, title, {$CATEGORY_CLASS}.{$UPPER_IDENTIFIER}.identifier);\r\n    }\r\n}\r\n";
        const string _menuTemplate = "package xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$IDENTIFIER};\r\n\r\nimport net.minecraft.core.BlockPos;\r\nimport net.minecraft.world.entity.player.Inventory;\r\nimport net.minecraft.world.entity.player.Player;\r\nimport net.minecraft.world.inventory.MenuType;\r\nimport org.jetbrains.annotations.Nullable;\r\nimport xkv.pluton.device.DeviceMenu;\r\nimport xkv.voltaic.device.{$CATEGORY_IDENTIFIER}.{$CATEGORY_CLASS};\r\n\r\npublic class {$CLASS_PREFIX}Menu extends DeviceMenu\r\n{\r\n    public {$CLASS_PREFIX}Menu(int _windowId, BlockPos _position, Inventory _inventory, Player _player)\r\n    {\r\n        super({$CATEGORY_CLASS}.{$UPPER_IDENTIFIER}.menu(), _windowId, _position, _inventory, _player);\r\n    }\r\n\r\n    @Override\r\n    protected void addDeviceSlots()\r\n    {\r\n\r\n    }\r\n}\r\n";

        const string _enumEntryFlag = "// <%DEVICE_ENTRY_TARGET>";

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

            FileInfo categoryFile = packageDir.File($"{data.CategoryClass}.java");

            if (categoryFile.Exists)
            {
                string categoryFileData = categoryFile.ReadString();

                categoryFile.WriteString(categoryFileData.Replace(_enumEntryFlag, $"{data.UpperIdentifier},\r\n    {_enumEntryFlag}"));
            }
        }

        #endregion

        #region Resource Generation

        const string _blockModelTemplate = "{\"parent\": \"block/cube_all\", \"textures\": {\"all\": \"voltaic:blocks/{$IDENTIFIER}\"}}";
        const string _blockStateTemplate = "{\r\n    \"variants\": {\r\n        \"facing=north,active=true\": { \"model\": \"voltaic:block/{$IDENTIFIER}\" }, \r\n        \"facing=east,active=true\": { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"y\": 90 }, \r\n        \"facing=south,active=true\": { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"y\": 180 }, \r\n        \"facing=west,active=true\": { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"y\": 270 }, \r\n        \"facing=up,active=true\": { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"x\": 270 }, \r\n        \"facing=down,active=true\": { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"x\": 90 }, \r\n        \"facing=north,active=false\": { \"model\": \"voltaic:block/{$IDENTIFIER}\" }, \r\n        \"facing=east,active=false\":  { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"y\": 90 }, \r\n        \"facing=south,active=false\": { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"y\": 180 }, \r\n        \"facing=west,active=false\":  { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"y\": 270 }, \r\n        \"facing=up,active=false\": { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"x\": 270 }, \r\n        \"facing=down,active=false\": { \"model\": \"voltaic:block/{$IDENTIFIER}\", \"x\": 90 }\r\n    }\r\n}\r\n";
        const string _itemModelTemplate = "{\"parent\" : \"voltaic:block/{$IDENTIFIER}\"}";

        const string _autoEntryTarget = "\"${AUTO_ENTRY_TARGET}\": \"\"";

        //xkv/voltaic/registry/RecipeRegistry.java

        void GenerateDeviceResources(DirectoryInfo resourceRoot, FieldValues data)
        {
            // Inject locale entry
            string locale = $"\"block.voltaic.{data.Identifier}\": \"{data.DisplayName}\",";

            TryWrite(resourceRoot.File($"assets\\voltaic\\models\\block\\{data.Identifier}.json"), () => _blockModelTemplate.Replace(_identifierKey, data.Identifier));
            TryWrite(resourceRoot.File($"assets\\voltaic\\models\\item\\{data.Identifier}.json"), () => _itemModelTemplate.Replace(_identifierKey, data.Identifier));
            TryWrite(resourceRoot.File($"assets\\voltaic\\blockstates\\{data.Identifier}.json"), () => _blockStateTemplate.Replace(_identifierKey, data.Identifier));

            FileInfo localeFile = resourceRoot.File($"assets\\voltaic\\lang\\en_us.json");

            if (localeFile.Exists)
            {
                string localeData = localeFile.ReadString();

                localeFile.WriteString(localeData.Replace(_autoEntryTarget, $"{locale}\r\n  {_autoEntryTarget}"));
            }
        }

        #endregion

        #region Format Setup Generation

        const string _autoFormatEntryTarget = "// ${AUTO_ENTRY_TARGET}";
        const string _autoFormatInitTarget = "// ${AUTO_INIT_TARGET}";

        const string _formatEntryTemplate = "public static RegistryObject<RecipeType<DeviceFormat>> {$UPPER_IDENTIFIER}_RECIPE;\r\n    public static RegistryObject<RecipeSerializer<?>> {$UPPER_IDENTIFIER}_SERIALIZER;\r\n";
        const string _formatInitTemplate = "{$UPPER_IDENTIFIER}_RECIPE = recipeType({$CATEGORY_CLASS}.{$UPPER_IDENTIFIER}.identifier);\r\n        {$UPPER_IDENTIFIER}_SERIALIZER = recipeSeralizer({$CATEGORY_CLASS}.{$UPPER_IDENTIFIER}.identifier, () -> new DeviceFormatSerializer( {$UPPER_IDENTIFIER}_RECIPE::get));\r\n";

        void GenerateFormatSetup(DirectoryInfo sourceRoot, DirectoryInfo resourceRoot, FieldValues data)
        {
            FileInfo registriesFile = sourceRoot.File("registry\\RecipeRegistry.java");

            if (registriesFile.Exists)
            {
                string fileData = registriesFile.ReadString();

                registriesFile.WriteString(fileData
                    .Replace(_autoFormatEntryTarget, $"{ReplaceFields(_formatEntryTemplate, data)}\r\n    {_autoFormatEntryTarget}")
                    .Replace(_autoFormatInitTarget, $"{ReplaceFields(_formatInitTemplate, data)}\r\n        {_autoFormatInitTarget}"));
            }

            DirectoryInfo formatFile = resourceRoot.Directory($"data\\voltaic\\recipes\\{data.Identifier}");

            if (!formatFile.Exists)
                formatFile.Create();
        }

        #endregion

        #region Serialization

        const string _lpd = "LastPackageDirectory";
        const string _lrr = "LastResourceRoot";

        public override JObject Export()
        {
            JObject data = new();

            if (!string.IsNullOrEmpty(_sourceDir.Value))
                data.Add(_lpd, _sourceDir.Value);

            if (!string.IsNullOrEmpty(_resourceRoot.Value))
                data.Add(_lrr, _resourceRoot.Value);

            return data;
        }

        public override void Import(JObject data)
        {
            if (data.ContainsKey(_lpd))
                _sourceDir.Value = data.GetString(_lpd);

            if (data.ContainsKey(_lrr))
                _resourceRoot.Value = data.GetString(_lrr);
        }

        #endregion
    }
}
