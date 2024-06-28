using System.Data.SQLite;
using FileMgr.Objects;
// Local libraries

// Overload for File to avoid conflicts with System.IO.File
using File = FileMgr.Objects.File;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FileMgr.Handlers;

/// <summary>
///     Manages relations between files and tags.
/// </summary>
public class Relations : BaseHandler
{
    /// <inheritdoc cref="BaseHandler" />
    public Relations(SQLiteConnection connection, ApplicationConfig config) : base(connection, config)
    {
    }


    /*************************************************************************************************************************************************************************************
     * Tag Returns
     *************************************************************************************************************************************************************************************/


    /// <summary>
    ///     Gets the tags on a file.
    /// </summary>
    /// <param name="fileId">File ID to get tags for.</param>
    /// <returns>List of tags.</returns>
    public List<Tag> GetTags(long fileId)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT tag_id FROM file_tags WHERE file_id = @file_id;", Connection);

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
            tags.Add(HandlersGroup.Tags!.Get(tagId)!);
        }

        // Close the reader and return the tags.
        reader.Close();
        return tags;
    }

    /// <summary>
    ///     Gets the tags on a file.
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
    ///     Gets all files with a tag.
    /// </summary>
    /// <param name="tag">Tag to get files with.</param>
    /// <returns>List of files with that tag.</returns>
    public List<File> GetFiles(Tag tag)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT file_id FROM file_tags WHERE tag_id = @tag_id;", Connection);

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
            files.Add(HandlersGroup.Files!.Get(fileId)!);
        }

        // Close the reader and return the files.
        reader.Close();
        return files;
    }

    /// <summary>
    ///     Adds a tag to a file.
    /// </summary>
    /// <param name="file">File to add tag to.</param>
    /// <param name="tag">Tag to add to file.</param>
    /// <returns>Updated file object.</returns>
    public File AddTag(File file, Tag tag)
    {
        // Create a new command.
        SQLiteCommand command = new("INSERT INTO file_tags (file_id, tag_id) VALUES (@file_id, @tag_id);", Connection);
        command.Parameters.AddWithValue("@file_id", file.Id);
        command.Parameters.AddWithValue("@tag_id", tag.Id);

        // Execute the command.
        command.ExecuteNonQuery();
        
        // Change the name of the file to include the tag 
        // Format:
        // - Original: "file.txt"
        // - New (assuming separator is &): "file.tag.txt" 
        // - And another tag would be "file.tag1&tag2.txt"
        
        // Split the file into 3 parts at each period
        string filename = file.Path.Split(Path.PathSeparator)[^1];

        string[] tags = [];
        string extension = filename.Split('.').Last();
        string name = filename.Split('.').First();
        
        if (filename.Count(x => x == '.') > 1)
        {
            // If there is more than one period, there are tags. (Note: The tags are not separated by periods, but by a user defined separator)
            tags = filename.Split('.')  // Now discard the first and last elements 
                .Skip(1).ToArray();

            tags = tags.Take(tags.Length - 1).ToArray();
            
            // Now recombine the tags array into a single string
            string tagString = string.Join("", tags);
            
            tags = tagString.Split(Config.Delimiter);
        }
        
        // Add the new tag to the tags array
        tags = tags.Append(tag.Name).ToArray();
        
        // Rejoin the tags array into a single string
        string newTagString = string.Join(Config.Delimiter, tags);
        
        // Rejoin the tags array into a single string
        string newFilename = $"{name}.{newTagString}.{extension}";
        
        // Rename the file
        System.IO.File.Move(file.Path, Path.Combine(file.Path, newFilename));
        file.Path = Path.Combine(file.Path, newFilename);
        HandlersGroup.Files!.Edit(file); // Update the file in the database
        
        // Return the file.
        return HandlersGroup.Files!.Get(file.Id)!;
    }


    /// <summary>
    ///     Removes a tag from a file.
    /// </summary>
    /// <param name="file">File to remove a tag from.</param>
    /// <param name="tag">Tag to remove.</param>
    /// <returns>Updated file object.</returns>
    public File RemoveTag(File file, Tag tag)
    {
        // Create a new command.
        SQLiteCommand command = new("DELETE FROM file_tags WHERE file_id = @file_id AND tag_id = @tag_id;", Connection);
        command.Parameters.AddWithValue("@file_id", file.Id);
        command.Parameters.AddWithValue("@tag_id", tag.Id);

        // Execute the command.
        command.ExecuteNonQuery();
        
        // Change the name of the file to remove the tag
        string[] tags = GetTagsFromFilename(file.Path);
        
        // Remove the tag from the tags array
        tags = tags.Where(x => x != tag.Name).ToArray();
        
        // Rename the file
        string newFilename = $"{file.Path.Split(Path.PathSeparator)[^1].Split('.').First()}.{string.Join(Config.Delimiter, tags)}.{file.Path.Split(Path.PathSeparator)[^1].Split('.').Last()}";
        
        // Rename the file
        System.IO.File.Move(file.Path, Path.Combine(file.Path, newFilename));
        file.Path = Path.Combine(file.Path, newFilename);
        HandlersGroup.Files!.Edit(file); // Update the file in the database
        
        // Return the file.
        return HandlersGroup.Files!.Get(file.Id)!;
    }
    
    private string[] GetTagsFromFilename(string filename)
    {
        // Split the file into 3 parts at each period
        string[] tags = [];
        string extension = filename.Split('.').Last();
        string name = filename.Split('.').First();

        if (filename.Count(x => x == '.') <= 1) return tags;
        
        // If there is more than one period, there are tags. (Note: The tags are not separated by periods, but by a user defined separator)
        tags = filename.Split('.')  // Now discard the first and last elements 
            .Skip(1).ToArray();

        tags = tags.Take(tags.Length - 1).ToArray();
            
        // Now recombine the tags array into a single string
        string tagString = string.Join("", tags);
            
        tags = tagString.Split(Config.Delimiter);

        return tags;
    }
}