using Api.Db.Models;

namespace Api.Db.Repos.Impml;

public class PgMediaRepo: IMediaRepo
{
    public Task<MediaDbm?> Get(Guid id)
    {
        throw new NotImplementedException();
    }
}