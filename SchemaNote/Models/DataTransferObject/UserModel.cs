using Newtonsoft.Json;

namespace SchemaNote.Models.DataTransferObject
{
    public class UserModel
    {
        public const string SessionKeyName = "_Name";
        public const string SessionKeyAge = "_Age";
        const string SessionKeyTime = "_Time";
        public string SessionInfo_Name { get; private set; }
        public string SessionInfo_Age { get; private set; }
        public string SessionInfo_CurrentTime { get; private set; }
        public string SessionInfo_SessionTime { get; private set; }
        [JsonProperty]
        public string SessionInfo_MiddlewareValue { get; private set; }

        public void SetMiddlewareValue(string _value)
        {
            SessionInfo_MiddlewareValue = _value;
        }
    }
}
