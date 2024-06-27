using System.Data.SQLite;
using FileMgr.Objects;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FileMgr.Handlers;

/// <summary>
///     Handles tag-related database operations.
/// </summary>
public class Tags
{
    /// <summary>
    ///     Database configuration.
    /// </summary>
    private readonly ApplicationConfig _config;

    /// <summary>
    ///     Low-level database connection.
    /// </summary>
    private readonly SQLiteConnection _connection;

    /// <summary>
    ///     Files handler.
    /// </summary>
    private Files _files;

    /// <summary>
    ///     Relations handler.
    /// </summary>
    private Relations _relations;

    /// <summary>
    ///     Tags handler constructor.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="config"></param>
    public Tags(SQLiteConnection connection, ApplicationConfig config)
    {
        _connection = connection;
        _config = config;
    }

    /// <summary>
    ///     Populates handlers. This is done separately as it must be done after all handlers are initialised.
    /// </summary>
    /// <param name="handlersGroup">Handlers group.</param>
    public void Populate(HandlersGroup handlersGroup)
    {
        _files = handlersGroup.Files;
        _relations = handlersGroup.Relations;
    }

    /// <summary>
    ///     Gets a tag by its ID.
    /// </summary>
    /// <param name="id">Tag ID</param>
    /// <returns>Tag object if found, null if not.</returns>
    public Tag? Get(long id)
    {
        // Create a query
        SQLiteCommand command = new("SELECT * FROM tags WHERE id = @id;", _connection);
        command.Parameters.AddWithValue("@id", id);

        // Get the data.
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();

        if (!reader.HasRows)
        {
            reader.Close();
            return null;
        }

        // Create a new tag object.
        Tag tag = new(reader);

        // Close the reader and return the tag.
        reader.Close();
        return tag;
    }

    /// <summary>
    ///     Get a tag by its name.
    /// </summary>
    /// <param name="name">Tag name.</param>
    /// <returns>Tag object if found, null if not.</returns>
    public Tag? Get(string name)
    {
        // Create query
        SQLiteCommand command = new("SELECT * FROM tags WHERE name = @tag;", _connection);
        command.Parameters.AddWithValue("@tag", name);

        // Get the data
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();

        if (!reader.HasRows)
        {
            reader.Close();
            return null;
        }

        // Create a new tag object.
        Tag tag = new(reader);

        // Close the reader and return the tag.
        reader.Close();
        return tag;
    }

    public List<Tag> GetSimilar(string name)
    {
        // Create query
        SQLiteCommand command = new("SELECT * FROM tags WHERE name LIKE @tag;", _connection);
        command.Parameters.AddWithValue("@tag", $"%{name}%");

        // Get the data
        SQLiteDataReader reader = command.ExecuteReader();
        List<Tag> tags = [];

        while (reader.Read()) tags.Add(new Tag(reader));

        // Close the reader and return the tags.
        reader.Close();
        return tags;
    }

    /// <summary>
    ///     Add a new tag to the database.
    /// </summary>
    /// <param name="name">Tag name.</param>
    /// <param name="colour">Tag colour.</param>
    public Tag New(
        string name,
        string colour = ""
    )
    {
        // Create a query.
        SQLiteCommand command = new("INSERT INTO tags (name, colour) VALUES (@tag, @colour) RETURNING id;", _connection);
        command.Parameters.AddWithValue("@tag", name);
        command.Parameters.AddWithValue("@colour", colour);

        // Execute the command and return the id.
        return Get((long)command.ExecuteScalar())!;
    }

    /// <summary>
    ///     Edits a tag in the database.
    /// </summary>
    /// <param name="tag">Tag object.</param>
    /// <returns>The updated tag object.</returns>
    public Tag Edit(ETag tag)
    {
        // Create a query.
        SQLiteCommand command = new("UPDATE tags SET name = @name, colour = @colour WHERE id = @id;", _connection);
        command.Parameters.AddWithValue("@name", tag.Name);
        command.Parameters.AddWithValue("@colour", tag.Colour);
        command.Parameters.AddWithValue("@id", tag.Id);

        // Execute the command and return the tag.
        command.ExecuteNonQuery();
        return Get(tag.Id)!;
    }

    /// <summary>
    ///     Delete a tag from the database.
    /// </summary>
    /// <param name="id">Tag ID.</param>
    public void Delete(long id)
    {
        // Create a query.
        SQLiteCommand command = new("DELETE FROM tags WHERE id = @id;", _connection);
        command.Parameters.AddWithValue("@id", id);

        // Execute the command.
        command.ExecuteNonQuery();
    }

    /// <summary>
    ///     Delete a tag from the database.
    /// </summary>
    /// <param name="tag">Tag Object.</param>
    public void Delete(Tag tag)
    {
        Delete(tag.Id);
    }
}