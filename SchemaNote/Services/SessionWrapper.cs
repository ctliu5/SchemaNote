using SchemaNote.Models.DataTransferObject;
using static SchemaNote.Models.Extensions.Extensions;

namespace SchemaNote.Services
{
    public interface ISessionWrapper
    {
        UserModel User { get; set; }

        // 清除整個 Session（含連線資訊）。
        void Clear();
    }

    public class SessionWrapper(IHttpContextAccessor httpContextAccessor) : ISessionWrapper
    {
        private static readonly string _userKey = "session.user";
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private ISession? Session
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Session;
            }
        }

        public UserModel User
        {
            get
            {
                return Session?.GetObject<UserModel>(_userKey) ?? new UserModel();
            }
            set
            {
                Session?.SetObject(_userKey, value);
            }
        }

        public void Clear()
        {
            Session?.Clear();
        }
    }
}
