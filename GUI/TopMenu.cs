﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using FileMgr;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace GUI;

public class TopMenu
{
    /// <summary>
    ///     File Managers. Key is the directory.
    /// </summary>
    public Common CommonData { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TopMenu"/> class.
    /// </summary>
    public TopMenu()
    {
        CommonData = Common.GetInstance();
    }
    
    public void TopMenu_File_Open_Click(object sender, RoutedEventArgs e)
    {
        // Open an explorer window to select a directory
        FolderBrowserDialog dialog = new();
        dialog.Description = "Select a directory to open";
        dialog.ShowNewFolderButton = true;
        dialog.RootFolder = Environment.SpecialFolder.MyComputer;

        if (dialog.ShowDialog() != DialogResult.OK) return;
        DirectoryInfo directory = new(dialog.SelectedPath);
        int actionCode; // 1 = open existing, 2 = create new, 3 is unused for this method

        // Check if there is a .tagster file in the directory
        if (!File.Exists(Path.Combine(directory.FullName, ".tagster")))
            actionCode = MessageBox.Show(
                "No .tagster file found in the directory. Would you like to create a new one?",
                "No .tagster file found",
                MessageBoxButton.YesNo
                ).Equals(MessageBoxResult.Yes) ? 2 : 0;
        else
            actionCode = 1;

        // Open the directory
        switch (actionCode)
        {
            case 1:
                CommonData.FileManagers.Add(new KeyValuePair<DirectoryInfo, FileManager>(directory, new FileManager(directory, actionCode)));
                break;
            case 2:
                CommonData.FileManagers.Add(new KeyValuePair<DirectoryInfo, FileManager>(directory, new FileManager(directory, actionCode)));
                break;
            default:
                return;
        }
    }


    public void TopMenu_File_Manage_Click(object sender, RoutedEventArgs e)
    {
        // Open a new window to manage active file managers
        ManageActiveFileManagers window = new();
        window.Show();
    }

    public void TopMenu_File_Exit_Click(object sender, RoutedEventArgs e)
    {
        // Cleanly exit all file managers
        foreach (KeyValuePair<DirectoryInfo, FileManager> managerSet in CommonData.FileManagers) managerSet.Value.Exit();

        // Close the application gracefully
        return; // This cannot be done from within this class as it is not the main window
    }
}