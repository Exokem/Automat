using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Automat
{
    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
        }

        void SelectItem(DropdownButton dropdown, MenuItem item)
        {
            dropdown.Content = item.Header;
            item.IsChecked = true;

            foreach (MenuItem otherItem in dropdown.DropdownMenu.Items)
            {
                if (otherItem != item)
                    otherItem.IsChecked = false;
            }
        }

        internal void Select(string mode)
        {
            if (mode == null)
                return;

            ModeDropdown.Content = mode;
            foreach (MenuItem otherItem in ModeDropdown.DropdownMenu.Items)
                otherItem.IsChecked = otherItem.Header.Equals(mode);
        }

        private void ConfigurationMode_Click(object sender, RoutedEventArgs e)
            => SelectItem(ModeDropdown, (MenuItem)sender);

        private void FileMode_Click(object sender, RoutedEventArgs e)
            => SelectItem(FileModeDropdown, (MenuItem)sender);
    }
}
