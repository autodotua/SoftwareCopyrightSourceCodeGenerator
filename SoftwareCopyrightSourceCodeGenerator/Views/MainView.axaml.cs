using Avalonia.Controls;
using Avalonia.Input;
using SoftwareCopyrightSourceCodeGenerator.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftwareCopyrightSourceCodeGenerator.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private MainViewModel viewModel => DataContext as MainViewModel;
    public void DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataFormats().Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Link;
        }
    }

    public void Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataFormats().Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Link;
        }
        var files = e.Data.GetFiles().Select(p => p.Path.LocalPath);
        viewModel.AddFilesOrFolder(files);
    }

    private async void BrowseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IReadOnlyList<Avalonia.Platform.Storage.IStorageItem> files = null;
        if (".".Equals((sender as Button).Tag))
        {
            files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                AllowMultiple = true,
            });
        }
        else
        {
            files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions()
            {
                AllowMultiple = true,
            });
        }
        if (files != null && files.Count > 0)
        {
            viewModel.AddFilesOrFolder(files.Select(p => p.Path.LocalPath));
        }

    }

    private void ListBox_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            foreach (var file in (sender as ListBox).SelectedItems.Cast<string>().ToList())
            {
                viewModel.Files.Remove(file);
            }
        }
    }

    private async void CopyButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(viewModel.Results);
    }
}
