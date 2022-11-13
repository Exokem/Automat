using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Automat.Plugins
{
    public static class ControlExtensions
    {
        public static int AddRow(this Grid grid, double proportion = 1D)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(proportion, GridUnitType.Star) });
            return grid.RowDefinitions.Count - 1;
        }

        public static int AddAbsoluteRow(this Grid grid, double size)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(size) });
            return grid.RowDefinitions.Count - 1;
        }

        public static int AddAutoRow(this Grid grid)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            return grid.RowDefinitions.Count - 1;
        }

        public static int AddColumn(this Grid grid, double proportion = 1D)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(proportion, GridUnitType.Star) });
            return grid.ColumnDefinitions.Count - 1;
        }

        public static int AddAbsoluteColumn(this Grid grid, double size)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(size) });
            return grid.ColumnDefinitions.Count - 1;
        }

        public static int AddAutoColumn(this Grid grid)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            return grid.ColumnDefinitions.Count - 1;
        }

        public static UIElement Place(this UIElement element, int r = -1, int c = -1, int rs = -1, int cs = -1)
        {
            if (0 <= r)
                Grid.SetRow(element, r);

            if (0 <= c)
                Grid.SetColumn(element, c);

            if (0 < rs)
                Grid.SetRowSpan(element, rs);

            if (0 < cs)
                Grid.SetColumnSpan(element, cs);

            return element;
        }
    }
}
