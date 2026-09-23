using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SchemaNote.Models.DataTransferObject;
using System.Security.Claims;

namespace SchemaNote.Services
{
    public interface IUserContext
    {
        UserModel User { get; set; }

        // 清除整個登入 Cookie（含連線資訊）。
        void Clear();
    }

    public class CookieUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        // 存放連線字串的 Claim 型別名稱。
        private static readonly string _connectionStringClaim = "SchemaNote.ConnectionString";
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public UserModel User
        {
            get
            {
                UserModel userModel = new();
                string? connectionString = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(_connectionStringClaim)?.Value;
                if (!string.IsNullOrEmpty(connectionString))
                {
                    userModel.SetConnectionString(connectionString);
                }
                return userModel;
            }
            set
            {
                HttpContext? httpContext = _httpContextAccessor.HttpContext;
                if (httpContext is null) return;

                List<Claim> claims = [];
                if (!string.IsNullOrEmpty(value.ConnectionString))
                {
                    claims.Add(new Claim(_connectionStringClaim, value.ConnectionString));
                }

                ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new(identity);

                // 以 Cookie-Authentication 發出登入 Cookie，將連線資訊存放在 Claims 中。
                httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
                    .GetAwaiter().GetResult();
            }
        }

        public void Clear()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            httpContext?.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                .GetAwaiter().GetResult();
        }
    }
}
