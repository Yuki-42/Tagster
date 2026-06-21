using System.Data.Common;
using NpgsqlTypes;

namespace Api.Db;

internal static class DbHelpers
{
	public const string TblAuditLogs = "audit.log";
	public const string TblUsers = "public.users";
	public const string TblApiKeys = "public.api_keys";
}