using ReciclaFacil.Application.Authentication;

namespace ReciclaFacil.Api.Security;

internal static class WebSessionCookie
{
    internal const string Name = "ReciclaFacil.Refresh";
    internal const string ClientHeader = "X-ReciclaFacil-Web";

    internal static bool IsWebClient(HttpRequest request) =>
        request.Headers.TryGetValue(ClientHeader, out var value) && value == "1";

    internal static string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(Name, out var token) ? token : null;

    internal static void Write(
        HttpResponse response,
        TokenPair pair,
        bool secure)
    {
        response.Cookies.Append(Name, pair.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Path = "/api/v1/auth/web",
            Expires = pair.RefreshTokenExpiresAt
        });
    }

    internal static void Delete(HttpResponse response, bool secure)
    {
        response.Cookies.Delete(Name, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Path = "/api/v1/auth/web"
        });
    }
}
