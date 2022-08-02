using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automat
{
    internal static class FileDialogUtility
    {
        static CommonOpenFileDialog GetDialog(string title, bool folder = false, bool multiselect = false, string from = null)
        {
            return new CommonOpenFileDialog()
            {
                Title = title,
                IsFolderPicker = folder,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = multiselect,
                ShowPlacesList = true,
                InitialDirectory = from
            };
        }

        internal static string SelectFolder(string title, string from = null)
        {
            using (var dialog = GetDialog(title, folder: true, from: from))
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    return dialog.FileName;
            }

            return null;
        }

        internal static IEnumerable<string> SelectFiles(string title, string from = null)
        {
            using (var dialog = GetDialog(title, multiselect: true, from: from))
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    foreach (var file in dialog.FileNames)
                        yield return file;
                }
            }

            yield break;
        }

        internal static string SelectFile(string title, string from = null)
        {
            using (var dialog = GetDialog(title, from: from))
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    return dialog.FileName;
                }
            }

            return null;
        }
    }
}
