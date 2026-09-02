using Newtonsoft.Json;

namespace SchemaNote.Models.DataTransferObject
{
    public class UserModel
    {
        [JsonProperty]
        public string? ConnectionString { get; private set; }

        public void SetConnectionString(string _value)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                // check if the connection string is not including "TrustServerCertificate=true", then add "TrustServerCertificate=true" to the connection string
                if (!_value.Contains("TrustServerCertificate", StringComparison.OrdinalIgnoreCase))
                {
                    // if the connection string is not ending with ";", then add ";" to the connection string
                    if (string.IsNullOrEmpty(_value) || _value.EndsWith(';'))
                    {
                        _value += "TrustServerCertificate=true;";
                    }
                    else
                    {
                        _value += ";TrustServerCertificate=true;";
                    }
                }
            }
            ConnectionString = _value;
        }
    }
}
