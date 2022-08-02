using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Automat
{
    public class DropdownButton : Button
    {
        public static readonly DependencyProperty DropdownMenuProperty
            = DependencyProperty.Register("DropdownMenu", typeof(ContextMenu), typeof(DropdownButton), new UIPropertyMetadata(MenuChanged));

        static void MenuChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (e.NewValue as ContextMenu).DataContext = (obj as DropdownButton).DataContext;
            (e.NewValue as ContextMenu).Width = (obj as DropdownButton).Width;
        }

        public ContextMenu DropdownMenu
        {
            get => (ContextMenu)GetValue(DropdownMenuProperty);
            set => SetValue(DropdownMenuProperty, value);
        }

        public DropdownButton()
        {
            DataContextChanged += (s, e) =>
            {
                if (DropdownMenu != null)
                    DropdownMenu.DataContext = DataContext;
            };
        }

        protected override void OnClick()
        {
            if (DropdownMenu == null)
                return;

            DropdownMenu.PlacementTarget = this;
            DropdownMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            DropdownMenu.Width = ActualWidth;
            DropdownMenu.IsOpen = true;

            foreach (MenuItem item in DropdownMenu.Items)
                item.IsChecked = item.Header.Equals(Content);
        }
    }
}
