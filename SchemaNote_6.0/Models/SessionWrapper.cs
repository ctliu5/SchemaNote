using Microsoft.AspNetCore.Http;
using static SchemaNote.Models.Extensions.Extensions;
using SchemaNote.Models.DataTransferObject;

namespace SchemaNote.Models
{
    public interface ISessionWrapper
    {
        UserModel User { get; set; }
    }

    public class SessionWrapper : ISessionWrapper
    {
        private static readonly string _userKey = "session.user";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionWrapper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session {
            get {
                return _httpContextAccessor.HttpContext.Session;
            }
        }

        public UserModel User {
            get {
                return Session.GetObject<UserModel>(_userKey) ?? new UserModel();
            }
            set {
                Session.SetObject(_userKey, value);
            }
        }
    }
}
