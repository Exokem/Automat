using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Automat.Plugins
{
    public abstract class Field : Notifier
    {
        public string Key { get; }

        /// <summary>
        /// The control used to edit this field.
        /// </summary>
        public abstract Control InputControl { get; }

        protected Field(string key)
        {
            Key = key;
        }
    }

    public abstract class Field<V> : Field
    {
        V _value;
        public V Value
        {
            get => _value;
            set => Notify(ref _value, value);
        }

        protected Field(string key) : base(key)
        {
        }
    }

    public class TextInputField : Field<string>
    {
        public override Control InputControl { get; }

        public TextInputField(string key) : base(key)
        {
            InputControl = new TextInputControl();
        }
    }

    public class DirectoryInputField : Field<string>
    {
        public override Control InputControl { get; }

        public ICommand SelectDirectoryCommand { get; }

        public DirectoryInputField(string key) : base(key)
        {
            InputControl = new DirectoryInputControl { DataContext = this };
            SelectDirectoryCommand = new Command(SelectDirectory);
        }

        void SelectDirectory()
        {
            var src = FileDialogUtility.SelectFolder("Select Folder", Value);

            if (src != null)
            {
                Value = src;
            }
        }
    }

    public class CheckInputField : Field<bool>
    {
        public override Control InputControl { get; }

        public CheckInputField(string key) : base(key)
        {
            InputControl = new CheckInputControl { DataContext = this };
        }
    }
}
