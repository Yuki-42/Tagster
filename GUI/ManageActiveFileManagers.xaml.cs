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
    private List<KeyValuePair<DirectoryInfo, FileManager>> _fileManagers;

    public ManageActiveFileManagers(ref List<KeyValuePair<DirectoryInfo, FileManager>> fileManagers)
    {
        _fileManagers = fileManagers;
        InitializeComponent();
        PopulateManagersList();
    }

    /// <summary>
    ///     Populates the list of active file managers.
    /// </summary>
    private void PopulateManagersList()
    {
        // Get the data grid
        DataGrid managersList = ManagersList;

        // Remove all rows
        managersList.Items.Clear();

        // Add the items
        foreach (KeyValuePair<DirectoryInfo, FileManager> fileManager in _fileManagers)
            managersList.Items.Add(new
            {
                Index = _fileManagers.IndexOf(fileManager),
                Uuid = fileManager.Value.Id,
                Directory = fileManager.Key.FullName,
                FileCount = fileManager.Value.Files.Count,
                TagCount = fileManager.Value.Tags.Count
            });
    }
    
    private void QuitButton_Click(object sender, RoutedEventArgs e)
    {
        // Check if there is a selected item
        if (ManagersList.SelectedItem == null)
        {
            return;
        }
        
        // Get the id of the selected item
        Guid uuid = (Guid) ((dynamic) ManagersList.SelectedItem).Uuid;
        
        // Get the file manager
        FileManager fileManager = _fileManagers.First(x => x.Value.Id == uuid).Value;
        
        // Close the file manager
        fileManager.Exit();
        
        // Remove the file manager from the list
        _fileManagers.Remove(_fileManagers.First(x => x.Value.Id == uuid));
        
        // Repopulate the list
        PopulateManagersList();
    }
}