using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using FileMgr;

namespace GUI;

public class Common : INotifyPropertyChanged
{
    /// <summary>
    /// Instance of the class.
    /// </summary>
    private static Common _instance = null;
    
    /// <summary>
    /// No idea what this is for.
    /// </summary>
    protected Common()
    {
    }

    /*************************************************************************************************************************************************************************************
     * Private fields
     *************************************************************************************************************************************************************************************/
    
    private string _title;  // TODO: Remove the title field, as it is only a template

    /// <summary>
    /// File Managers. Key is the directory.
    /// </summary>
    private ObservableCollection<KeyValuePair<DirectoryInfo, FileManager>> _fileManagers;
    
    /*************************************************************************************************************************************************************************************
     * Exposed properties
     *************************************************************************************************************************************************************************************/


    /// <inheritdoc cref="_fileManagers"/>
    public ObservableCollection<KeyValuePair<DirectoryInfo, FileManager>> FileManagers
    {
        get => _fileManagers;
        set
        {
            if (_fileManagers == value) return;
            _fileManagers = value;
            OnPropertyChanged(nameof(FileManagers));
        }
    }
    
    public string Title
    {
        get => _title;
        set
        {
            if (_title == value)
                return;
            _title = value;
            OnPropertyChanged(nameof(Title));
        }
    }

    /*************************************************************************************************************************************************************************************
     * Copy-pasted code
     *
     * Source: 
     * https://stackoverflow.com/a/23189312/19260873
     *************************************************************************************************************************************************************************************/
    
    /// <summary>
    /// No idea what this is for.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Gets the current active instance of the class.
    /// </summary>
    /// <returns>Instance</returns>
    public static Common GetInstance()
    {
        if (_instance == null)
            _instance = new Common();
        return _instance;
    }

    public void Load()
    {
    }

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventArgs ea = new(propertyName);
        if (PropertyChanged != null)
            PropertyChanged(this, ea);
    }
}