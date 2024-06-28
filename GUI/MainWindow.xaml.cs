using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FileMgr;

// Project-specific

namespace GUI;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    ///     Shared top menu code.
    /// </summary>
    private readonly TopMenu _topMenu;

    /// <summary>
    ///     File Managers. Key is the directory.
    /// </summary>
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ObservableCollection<KeyValuePair<DirectoryInfo, FileManager>> _fileManagers;
    
    public MainWindow()
    {
        InitializeComponent();
        _fileManagers = new ObservableCollection<KeyValuePair<DirectoryInfo, FileManager>>();
        _topMenu = new TopMenu(ref _fileManagers);
    }

    // Expose TopMenu events
    private void TopMenu_File_Open_Click(object sender, RoutedEventArgs e)
    {
        _topMenu.TopMenu_File_Open_Click(sender, e);
    }

    private void TopMenu_File_Manage_Click(object sender, RoutedEventArgs e)
    {
        _topMenu.TopMenu_File_Manage_Click(sender, e);
    }

    private void TopMenu_File_Exit_Click(object sender, RoutedEventArgs e)
    {
        _topMenu.TopMenu_File_Exit_Click(sender, e);
        Close();
    }

    private TabItem ConstructManagerTab(FileManager manager)
    {
        // Create the tab item
        TabItem tabItem = new()
        {
            Header = manager.FriendlyName
        };
        return tabItem;
    }
}