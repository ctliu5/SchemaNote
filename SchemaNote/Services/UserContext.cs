using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using SchemaNote.Constants;
using System.Security.Claims;

namespace SchemaNote.Services;

public interface IUserContext
{
    public string Init(string connectionString);

    public string GetConnectionString(out bool found);

    // 清除整個登入 Cookie（含連線資訊）。
    void Clear();
}

public class CookieUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    // 存放連線字串的 Claim 型別名稱。
    private static readonly string _connectionStringClaim = Common.ConnectionStringClaim;
    // 固定的使用者識別值。antiforgery 會依身分的識別 Claim 計算權杖綁定值，
    // 已驗證身分若缺少識別 Claim，會導致防偽權杖驗證不一致（首次 POST 失敗）。
    private static readonly string _userIdClaim = Common.UserIdClaim;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string Init(string connectionString)
    {
        if (!string.IsNullOrEmpty(connectionString))
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                TrustServerCertificate = true
            };
            connectionString = builder.ConnectionString;
        }

        HttpContext httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is null.");
        if (httpContext.User.Identity?.IsAuthenticated is true)
        {
            // 已登入，先清除舊的登入資訊。
            httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                .GetAwaiter().GetResult();
        }
        // 提供穩定的識別 Claim，讓 antiforgery 綁定的身分保持一致。
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, _userIdClaim),
                new Claim(ClaimTypes.Name, _userIdClaim),
            ];
        if (!string.IsNullOrEmpty(connectionString))
        {
            claims.Add(new Claim(_connectionStringClaim, connectionString));
        }

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        // 以 Cookie-Authentication 發出登入 Cookie，將連線資訊存放在 Claims 中。
        httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
            .GetAwaiter().GetResult();

        return connectionString;
    }

    public string GetConnectionString(out bool found)
    {
        string? connectionString = _httpContextAccessor.HttpContext?.User?
            .FindFirst(_connectionStringClaim)?.Value;
        found = !string.IsNullOrEmpty(connectionString);
        return connectionString ?? string.Empty;
    }

    public void Clear()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        httpContext?.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
            .GetAwaiter().GetResult();
    }
}
