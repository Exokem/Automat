using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Automat
{
    public class Configuration : INotifyPropertyChanged
    {
        public readonly ConfigurationView View;
        public event PropertyChangedEventHandler? PropertyChanged;

        Dictionary<string, (bool blacklisted, bool whitelisted)> _filterConfiguration;

        HashSet<string> _blacklist, _whitelist;

        HashSet<string> FileList => ScanMode == "Whitelist" ? _whitelist : _blacklist;

        static readonly SolidColorBrush DarkWhite = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8e8e8"));

        #region Binding Properties

        DirectoryInfo _source;
        public string Source
        {
            get => _source.FullName;
            set => Set(ref _source, new DirectoryInfo(value));
        }

        DirectoryInfo _destination;
        public string Destination
        {
            get => _destination.FullName;
            set => Set(ref _destination, new DirectoryInfo(value));
        }

        string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        string _fileMode = "Blacklist";
        public string ScanMode
        {
            get => _fileMode;
            set => Set(ref _fileMode, value);
        }

        string _configurationMode = "<Select Mode>";
        public string ConfigurationMode
        {
            get => _configurationMode;
            set => Set(ref _configurationMode, value);
        }

        public ICommand RunConfiguration => new Command(Run);
        public ICommand SelectSourceCommand => new Command(SelectSource);
        public ICommand SelectDestinationCommand => new Command(SelectDestination);

        public ICommand ModeChangedCommand => new Command(ScanModeChanged);

        public ICommand SelectFilesCommand => new Command(SelectFiles);
        public ICommand ClearFilesCommand => new Command(ClearFiles);

        #endregion

        public Configuration()
        {
            View = new ConfigurationView();
            View.DataContext = this;

            _filterConfiguration = new Dictionary<string, (bool, bool)>();
            _blacklist = new HashSet<string>();
            _whitelist = new HashSet<string>();
        }

        public Configuration(JObject data) : this()
        {
            Name = (string) data["Name"];
            Source = (string) data["Source"];
            Destination = (string) data["Destination"];
            ConfigurationMode = (string) data["ConfigurationMode"];
            ScanMode = (string)data[nameof(ScanMode)];

            _whitelist = ((JArray)data["Whitelist"]).ToCollection(new HashSet<string>());
            _blacklist = ((JArray)data["Blacklist"]).ToCollection(new HashSet<string>());

            ScanModeChanged();
        }

        Grid FileDisplay(string file, int i)
        {
            Grid grid = new Grid();

            TextBlock text = new TextBlock 
            { 
                Text = file, VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(6, 0, 0, 0)
            };
            Button remove = new Button 
            { 
                Content = "Remove",
                Margin = new Thickness(5),
            };
            remove.Click += (s, e) => RemoveFile(file);

            if (i % 2 == 0)
                grid.Background = DarkWhite;

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(remove, 2);

            grid.Children.Add(text);
            grid.Children.Add(remove);

            return grid;
        }

        void ScanModeChanged()
        {
            View.FileList.Children.Clear();

            int i = 0;

            foreach (var file in FileList)
                View.FileList.Children.Add(FileDisplay(file, i++));

            DataManager.Save();
        }

        void SelectFiles()
        {
            foreach (var file in FileDialogUtility.SelectFiles($"Select Files for {ScanMode}", Source))
            {
                if (!FileList.Contains(file))
                    FileList.Add(file);
            }

            ScanModeChanged();
        }

        private void ClearFiles()
        {
            FileList.Clear();
            ScanModeChanged();
        }

        void RemoveFile(string file)
        {
            FileList.Remove(file);
            ScanModeChanged();
        }

        void SelectSource()
        {
            var src = FileDialogUtility.SelectFolder("Select Source Folder", Source);

            if (src != null)
            {
                Source = src;
                DataManager.Save();
            }
        }

        void SelectDestination()
        {
            var src = FileDialogUtility.SelectFolder("Select Destination Folder", Destination);

            if (src != null)
            {
                Destination = src;
                DataManager.Save();
            }
        }

        void Run()
        {
            IConfigurationMode.Execute(ConfigurationMode, _source, _destination);
        }

        void Set<V>(ref V v, V value, [CallerMemberName] string property = null)
        {
            if (v != null && v.Equals(value))
                return;

            v = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public JObject Export()
        {
            JObject data = new JObject();

            data.Add("Name", Name);
            data.Add("Source", Source);
            data.Add("Destination", Destination);
            data.Add("ConfigurationMode", ConfigurationMode);
            data.Add(nameof(ScanMode), ScanMode);

            data.Add("Whitelist", _whitelist.ToJArray());
            data.Add("Blacklist", _blacklist.ToJArray());

            return data;
        }
    }
}
