using System.Data.SQLite;

// Local libraries
using FileMgr.Objects;

// Overload for File to avoid conflicts with System.IO.File
using File = FileMgr.Objects.File;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FileMgr.Handlers;

/// <summary>
/// Manages relations between files and tags.
/// </summary>
public class Relations
{
    /// <summary>
    /// Low-level database connection.
    /// </summary>
    private readonly SQLiteConnection _connection;

    /// <summary>
    /// Database configuration.
    /// </summary>
    private readonly ApplicationConfig _config;
    
    /// <summary>
    /// Files handler.
    /// </summary>
    private Files _files;
    
    /// <summary>
    /// Tags handler.
    /// </summary>
    private Tags _tags;
    

    /// <summary>
    /// Constructor for the Relations handler.
    /// </summary>
    /// <param name="connection">Database connection.</param>
    /// <param name="config">App config.</param>
    public Relations(
        SQLiteConnection connection,
        ApplicationConfig config
        )
    {
        _connection = connection;
        _config = config;
    }

    /// <summary>
    /// Populates handlers. This is done separately as it must be done after all handlers are initialised.
    /// </summary>
    /// <param name="handlersGroup">Handlers group.</param>
    public void Populate(HandlersGroup handlersGroup)
    {
        _files = handlersGroup.Files;
        _tags = handlersGroup.Tags;
    }

    /*************************************************************************************************************************************************************************************
     * Tag Returns
     *************************************************************************************************************************************************************************************/

    
    /// <summary>
    /// Gets the tags on a file.
    /// </summary>
    /// <param name="fileId">File ID to get tags for.</param>
    /// <returns>List of tags.</returns>
    public List<Tag> GetTags(long fileId)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT tag_id FROM file_tags WHERE file_id = @file_id;", _connection);

        // Add the parameter.
        command.Parameters.AddWithValue("@file_id", fileId);

        // Execute the command and get the reader.
        SQLiteDataReader reader = command.ExecuteReader();

        // Create a list of tags.
        List<Tag> tags = [];

        // Read the data.
        while (reader.Read())
        {
            long tagId = reader.GetInt64(0);
            tags.Add(new Tags(_connection, _config).Get(tagId)!);
        }

        // Close the reader and return the tags.
        reader.Close();
        return tags;
    }

    /// <summary>
    /// Gets the tags on a file.
    /// </summary>
    /// <param name="file">File object to get tags for.</param>
    /// <returns>List of tags.</returns>
    public List<Tag> GetTags(File file)
    {
        return GetTags(file.Id);
    }
    
    /*************************************************************************************************************************************************************************************
     * File Returns
     *************************************************************************************************************************************************************************************/
    
    /// <summary>
    /// Gets all files with a tag.
    /// </summary>
    /// <param name="tag">Tag to get files with.</param>
    /// <returns>List of files with that tag.</returns>
    public List<File> GetFiles(Tag tag)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT file_id FROM file_tags WHERE tag_id = @tag_id;", _connection);

        // Add the parameter.
        command.Parameters.AddWithValue("@tag_id", tag.Id);

        // Execute the command and get the reader.
        SQLiteDataReader reader = command.ExecuteReader();

        // Create a list of files.
        List<File> files = [];

        // Read the data.
        while (reader.Read())
        {
            long fileId = reader.GetInt64(0);
            files.Add(_files.Get(fileId)!);
        }

        // Close the reader and return the files.
        reader.Close();
        return files;
    }
    
    /// <summary>
    /// Adds a tag to a file.
    /// </summary>
    /// <param name="file">File to add tag to.</param>
    /// <param name="tag">Tag to add to file.</param>
    /// <returns>Updated file object.</returns>
    public File AddTag(File file, Tag tag)
    {
        // Create a new command.
        SQLiteCommand command = new("INSERT INTO file_tags (file_id, tag_id) VALUES (@file_id, @tag_id);", _connection);
        command.Parameters.AddWithValue("@file_id", file.Id);
        command.Parameters.AddWithValue("@tag_id", tag.Id);
        
        // Execute the command.
        command.ExecuteNonQuery();
        
        // Return the file.
        return _files.Get(file.Id)!;
    }
    
    
    /// <summary>
    /// Removes a tag from a file.
    /// </summary>
    /// <param name="file">File to remove a tag from.</param>
    /// <param name="tag">Tag to remove.</param>
    /// <returns>Updated file object.</returns>
    public File RemoveTag(File file, Tag tag)
    {
        // Create a new command.
        SQLiteCommand command = new("DELETE FROM file_tags WHERE file_id = @file_id AND tag_id = @tag_id;", _connection);
        command.Parameters.AddWithValue("@file_id", file.Id);
        command.Parameters.AddWithValue("@tag_id", tag.Id);
        
        // Execute the command.
        command.ExecuteNonQuery();
        
        // Return the file.
        return _files.Get(file.Id)!;
    }
}