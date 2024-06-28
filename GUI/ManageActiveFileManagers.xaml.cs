using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FileMgr;

namespace GUI;

public partial class ManageActiveFileManagers : Window
{
    /// <summary>
    ///     File Managers. Key is the directory.
    /// </summary>
    public ObservableCollection<KeyValuePair<DirectoryInfo, FileManager>> FileManagers { get; set; }

    public ManageActiveFileManagers(ref ObservableCollection<KeyValuePair<DirectoryInfo, FileManager>> fileManagers)
    {
        FileManagers = fileManagers;
        InitializeComponent();
        //PopulateManagersList();
    }

    ///// <summary>
    /////     Populates the list of active file managers.
    ///// </summary>
    //private void PopulateManagersList()
    //{
    //    // Get the data grid
    //    DataGrid managersList = ManagersList;

    //    // Remove all rows
    //    managersList.Items.Clear();

    //    // Add the items
    //    foreach (KeyValuePair<DirectoryInfo, FileManager> fileManager in FileManagers)
    //        managersList.Items.Add(new
    //        {
    //            Index = FileManagers.IndexOf(fileManager),
    //            Uuid = fileManager.Value.Id,
    //            Directory = fileManager.Key.FullName,
    //            FileCount = fileManager.Value.Files.Count,
    //            TagCount = fileManager.Value.Tags.Count
    //        });
    //}
    
    private void QuitButton_Click(object sender, RoutedEventArgs e)
    {
        // Check if there is a selected item
        if (ManagersList.SelectedItem == null)
        {
            return;
        }
        
        // Get the selected item 
        dynamic selectedItem = ManagersList.SelectedItem;  // This is a KeyValuePair<DirectoryInfo, FileManager>
        
        Debug.Assert(selectedItem is KeyValuePair<DirectoryInfo, FileManager>);
        
        // Get the id of the selected item
        Guid uuid = selectedItem.Value.Id;
        
        // Get the file manager
        FileManager fileManager = FileManagers.First(x => x.Value.Id == uuid).Value;
        
        // Close the file manager
        fileManager.Exit();
        
        // Remove the file manager from the list
        FileManagers.Remove(FileManagers.First(x => x.Value.Id == uuid));
    }
}