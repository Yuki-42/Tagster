using Api.Db.Models;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace Api.Db.Repos.Impml;

/// <inheritdoc />
public class PgAuditLogRepo(IDbConnectionProvider con) : IAuditLogRepo
{
	/// <inheritdoc />
	public async Task<AuditLogDbm> Create(CreateAuditLogDbo ob)
	{
		// Get db connection
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		// Create command
		const string cmd = "INSERT INTO audit.log (timestamp, table_name, action_type, row_id, user_id, previous_state, effected, comment) VALUES (@Timestamp, @TableName, @ActionType, @RowId, @UserId, @PreviousState, @Effected, @Comment) RETURNING *;";

		// Execute command
		return await db.QueryFirstAsync<AuditLogDbm>(cmd, ob);
	}
}