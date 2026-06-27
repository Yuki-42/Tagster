using System.Text;

namespace Api.Db.Models;

/// <summary>
/// DBM For environment config row.
///
/// This is used to allow the server to swap out variables at runtime.
/// </summary>
public class EnvironmentDbm
{
    public const string PrivateKey = "private_key";
    public const string OtpSecret = "otp_secret";

    /// <summary>
    /// Possible data types stored w
    /// </summary>
    public enum DataType
    {
        ByteArray,
        String,
        Int
    }
    
    /// <summary>
    /// Config key.
    /// </summary>
    public required string Key { get; init; }
    
    /// <summary>
    /// Type of data.
    /// </summary>
    public required DataType DType { get; init; }
    
    /// <summary>
    /// Actual data contents.
    /// </summary>
    public required byte[] Data { get; init; }

    /// <summary>
    /// Environment value as string.
    /// </summary>
    public string AsString =>
        // Convert bytea to string
        Encoding.UTF8.GetString(Data);
}