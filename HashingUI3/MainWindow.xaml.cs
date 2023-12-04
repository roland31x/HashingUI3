using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Security.Cryptography;
using Windows.Storage.Pickers;
using System.Runtime.InteropServices;
using WinRT;
using Windows.Storage;
using System.Text;
using Microsoft.UI.Xaml.Documents;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HashingUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        HashAlgorithm algorithm;
        List<string> SelectedFiles = new List<string>();
        List<byte[]> Hashes = new List<byte[]>();
        public MainWindow()
        {
            this.InitializeComponent();
            (HashSelectionBox.Items[0] as ComboBoxItem).Tag = MD5.Create();
            (HashSelectionBox.Items[1] as ComboBoxItem).Tag = SHA1.Create();
            (HashSelectionBox.Items[2] as ComboBoxItem).Tag = SHA256.Create();
            (HashSelectionBox.Items[3] as ComboBoxItem).Tag = SHA384.Create();
            (HashSelectionBox.Items[4] as ComboBoxItem).Tag = SHA512.Create();
            algorithm = MD5.Create();
            Hash_Button.IsEnabled = false;
        }

        private async void Hash_Button_Click(object sender, RoutedEventArgs e)
        {
            Hash_Button.IsEnabled = false;
            FileSelectorButton.IsEnabled = false;
            await ComputeHashes();
            Hash_Button.IsEnabled = true;
            FileSelectorButton.IsEnabled = true;
        }
        Task ComputeHashes()
        {
            HashBlock.Inlines.Clear();

            foreach (string file in SelectedFiles)
            {
                byte[] hash = algorithm.ComputeHash(GetBytes(file));
                Hashes.Add(hash);
            }
            foreach (byte[] hash in Hashes)
                HashBlock.Inlines.Add(new Run() { Text = hash.ToHexString() + Environment.NewLine });

            Hashes.Clear();

            return Task.CompletedTask;
        }
            
        byte[] GetBytes(string file)
        {
            return File.ReadAllBytes(file);
        }

        private void HashSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            algorithm = (HashSelectionBox.SelectedItem as ComboBoxItem).Tag as HashAlgorithm;
        }

        private async void FileSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add("*");

            IInitializeWithWindow withWindow = picker.As<IInitializeWithWindow>();
            IntPtr handle = this.As<IWindowNative>().WindowHandle;
            withWindow.Initialize(handle);

            var files = await picker.PickMultipleFilesAsync();
            if(files != null)
            {
                Hash_Button.IsEnabled = true;
                FileNameBlock.Inlines.Clear();
                SelectedFiles.Clear();
                HashBlock.Inlines.Clear();
                Hashes.Clear();
                foreach (StorageFile file in files)
                {
                    FileNameBlock.Inlines.Add(new Run() { Text = file.Path.Split('\\').Last() + Environment.NewLine });
                    SelectedFiles.Add(file.Path);
                }
            }
        }

        #region Workaround stuff
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        internal interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
        #endregion
    }
    public static class ByteArrayExt
    {
        public static string ToHexString(this byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}
