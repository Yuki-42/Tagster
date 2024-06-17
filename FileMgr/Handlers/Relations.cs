using System.Data.SQLite;
using FileMgr.Handlers;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FileMgr.Objects;

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
            files.Add(new Files(_connection, _config).Get(fileId)!);
        }

        // Close the reader and return the files.
        reader.Close();
        return files;
    }
    
    public 
    
}