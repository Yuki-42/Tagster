namespace FileMgr.Handlers;

/// <summary>
///     Groups handlers together to allow for shared access.
/// </summary>
public class HandlersGroup
{
    /// <summary>
    /// Initialises a new instance of the <see cref="HandlersGroup"/> class.
    /// </summary>
    /// <param name="files">Files Handler.</param>
    /// <param name="tags">Tags Handler.</param>
    /// <param name="relations">Relations Handler.</param>
    public HandlersGroup(Files files, Tags tags, Relations relations)
    {
        Files = files;
        Tags = tags;
        Relations = relations;
    }

    /// <summary>
    ///     Files handler.
    /// </summary>
    public Files Files { get; }

    /// <summary>
    ///     Tags handler.
    /// </summary>
    public Tags Tags { get; }

    /// <summary>
    ///     Relations Handler.
    /// </summary>
    public Relations Relations { get; }
}