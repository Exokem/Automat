using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Automat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataManager.LoadConfigurations(AddConfiguration);
        }

        void AddConfiguration(Configuration cfg)
        {
            ConfigurationHolder.Children.Add(cfg.View);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DataManager.Save();
            
            base.OnClosing(e);
        }

        private void AddConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationSetupDialog dialog = new ConfigurationSetupDialog
            {
                Owner = this,
            };

            if (dialog.ShowDialog().GetValueOrDefault(false))
            {
                Configuration config = new Configuration
                {
                    Name = dialog.NameValue,
                    Source = dialog.SourceValue,
                    Destination = dialog.DestinationValue,
                };

                DataManager.SaveConfiguration(config);

                ConfigurationHolder.Children.Add(config.View);
            }
        }
    }
}
