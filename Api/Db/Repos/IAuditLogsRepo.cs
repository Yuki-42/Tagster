using Api.Db.Models;

namespace Api.Db.Repos;

public interface IAuditLogsRepo
{
	Task<AuditLogDbm> Create(CreateAuditLogDbo ob);
}