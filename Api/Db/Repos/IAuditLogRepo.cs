using Api.Db.Models;

namespace Api.Db.Repos;

public interface IAuditLogRepo
{
	Task<AuditLogDbm> Create(CreateAuditLogDbo ob);
}