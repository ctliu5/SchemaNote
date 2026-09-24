using Microsoft.Data.SqlClient;

namespace SchemaNote.Services;
/// <summary>從連線字串解析出可供前端比對的連線資訊（Server / Database）。</summary>
public interface IConnectionInfoService
{
    /// <summary>
    /// 解析連線字串取得 Server Address 與 Database Name；無法解析時回傳空字串。
    /// </summary>
    (string Server, string Database) Parse(string connectionString);
}

public class ConnectionInfoService : IConnectionInfoService
{
    public (string Server, string Database) Parse(string connectionString)
    {
        string server = string.Empty;
        string database = string.Empty;
        try
        {
            SqlConnectionStringBuilder builder = new(connectionString);
            server = builder.DataSource ?? string.Empty;
            database = builder.InitialCatalog ?? string.Empty;
#if DEBUG
            _ = builder.UserID;
            _ = builder.Password;
#endif
        }
        catch
        {
            // 無法解析時維持空字串，前端會視為沒有可比對的連線資訊。
        }
        return (server, database);
    }
}
