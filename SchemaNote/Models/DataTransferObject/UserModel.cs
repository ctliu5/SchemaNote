using Microsoft.Data.SqlClient;

namespace SchemaNote.Models.DataTransferObject;

public class UserModel
{
    public string? ConnectionString { get; private set; }

    public void SetConnectionString(string _value)
    {
        if (!string.IsNullOrEmpty(_value))
        {
            var builder = new SqlConnectionStringBuilder(_value)
            {
                TrustServerCertificate = true
            };
            _value = builder.ConnectionString;
        }
        ConnectionString = _value;
    }
}
