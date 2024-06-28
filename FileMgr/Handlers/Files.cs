using System.Data.SQLite;
using FileMgr.Objects;
using File = FileMgr.Objects.File;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FileMgr.Handlers;

/// <summary>
///     Handles file-related database operations.
/// </summary>
public class Files : BaseHandler
{
    /// <inheritdoc cref="BaseHandler" />
    public Files(SQLiteConnection connection, ApplicationConfig config) : base(connection, config)
    {
    }

    /// <summary>
    ///     Gets the number of files in the database.
    /// </summary>
    /// <returns>Number of files.</returns>
    public int Count
    {
        get
        {
            // Create a new command.
            SQLiteCommand command = new("SELECT COUNT(*) FROM files;", Connection);

            // Execute the command and get the count.
            return (int)(long)command.ExecuteScalar();
        }
    }

    /*************************************************************************************************************************************************************************************
     * Database Operations
     *************************************************************************************************************************************************************************************/

    /// <summary>
    ///     Gets a file by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>File if found.</returns>
    public File? Get(long id)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT * FROM files WHERE id = @id;", Connection);
        command.Parameters.AddWithValue("@id", id);

        using SQLiteDataReader reader = command.ExecuteReader();

        if (!reader.HasRows) return null;

        // Read the data.
        reader.Read();
        long fileId = reader.GetInt64(0);

        // Create a new file object and return it
        return new File(reader, HandlersGroup.Relations!.GetTags(fileId));
    }


    /// <summary>
    ///     Adds a file to the database.
    /// </summary>
    /// <param name="file">File to add.</param>
    /// <returns>File added.</returns>
    public File Add(FileInfo file)
    {
        // Create a new command 
        SQLiteCommand command = new("INSERT INTO files (path) VALUES (@path) RETURNING id;", Connection);
        command.Parameters.AddWithValue("@path", file.FullName);

        // Execute the command and get the ID
        return Get((long)command.ExecuteScalar())!;
    }

    /// <summary>
    ///     Adds a file to the database with tags.
    /// </summary>
    /// <param name="file">File to add.</param>
    /// <param name="tags">Tags to add.</param>
    /// <returns>File added.</returns>
    public File Add(
        FileInfo file,
        List<Tag> tags
    )
    {
        // Add the file
        File newFile = Add(file);

        // Add the tags
        foreach (Tag tag in tags) HandlersGroup.Relations!.AddTag(newFile, tag);

        return newFile;
    }

    
    /// <summary>
    /// Edits the file in the database. Only really used for changing the path.
    /// </summary>
    /// <param name="file">Modified file object.</param>
    /// <returns></returns>
    public File Edit(File file)
    {
        // Create a new command.
        SQLiteCommand command = new("UPDATE files SET path = @path WHERE id = @id;", Connection);
        command.Parameters.AddWithValue("@path", file.Path);
        command.Parameters.AddWithValue("@id", file.Id);
        
        // Execute the command.
        command.ExecuteNonQuery();
        
        return Get(file.Id)!;
    }
}