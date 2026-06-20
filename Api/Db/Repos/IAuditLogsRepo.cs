using Api.Db.Models;

namespace Api.Db.Repos;

public interface IAuditLogsRepo
{
	Task<AuditLogDbm> Create(CreateAuditLogDbo ob);

	/// <summary>
	/// Add a log call to the queue. 
	/// </summary>
	/// <param name="task"></param>
	void Enqueue(Task<AuditLogDbm> task);
}