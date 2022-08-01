using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Automat
{
    /// <summary>
    /// Interaction logic for ConfigurationSetupDialog.xaml
    /// </summary>
    public partial class ConfigurationSetupDialog : Window
    {
        public string NameValue => NameHolder.Text;
        public string SourceValue => SourceHolder.Text;
        public string DestinationValue => DestinationHolder.Text;
        
        public ConfigurationSetupDialog()
        {
            InitializeComponent();
        }

        private void SelectSourceButton_Click(object sender, RoutedEventArgs e)
        {
            SourceHolder.Text = FileDialogUtility.SelectFolder("Select Source Folder");
        }

        private void SelectDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            DestinationHolder.Text = FileDialogUtility.SelectFolder("Select Destination Folder");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (NameValue.Length != 0 && SourceValue.Length != 0 && DestinationValue.Length != 0)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
