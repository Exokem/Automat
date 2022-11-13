using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Automat.Plugins
{
    public sealed class Form
    {
        public static Form Assemble(IEnumerable<Field> fields, FormInformation information = default)
        {
            Border border = new Border
            {
                Padding = new Thickness(5),
            };

            Grid grid = new();

            grid.AddAutoColumn();
            grid.AddAbsoluteColumn(5);
            grid.AddColumn();

            Form form = new(grid);

            foreach (var field in fields)
            {
                form._fields[field.Key] = field;

                if (grid.Children.Count != 0 && information.FieldSpacing != 0)
                    grid.AddAbsoluteRow(information.FieldSpacing);

                int r = grid.AddAutoRow();

                grid.Children.Add(AssembleLabel(field).Place(r: r));
                grid.Children.Add(field.InputControl.Place(r: r, c: 2));
            }

            return form;
        }

        static TextBlock AssembleLabel(Field field)
        {
            return new TextBlock { Text = $"{field.Key}:", Padding = new Thickness(5) };
        }

        readonly Dictionary<string, Field> _fields = new();

        public UIElement View { get; }

        Form(UIElement view)
        {
            View = view;
        }

        public bool GetField<V>(string key, out Field<V> field)
        {
            if (_fields.ContainsKey(key) && _fields[key] is Field<V> f)
            {
                field = f;
                return true;
            }

            field = null;
            return false;
        }
    }

    public readonly struct FormInformation
    {
        /// <summary>
        /// The vertical space between each field in the form.
        /// </summary>
        public int FieldSpacing { get; init; }
    }
}
