using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Application.Common.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fayora.Api.Services;

public class ClientContextProvider(IHttpContextAccessor accessor) : IClientContextProvider
{
    public ClientContext GetContext()
    {
        var context = accessor.HttpContext;
        if (context == null) return default!;

        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "Unknown";

        var deviceId = GetClaimsValue("device_id");

        var email = GetClaimsValue(JwtRegisteredClaimNames.Email);
        var phoneNumber = GetClaimsValue("phone");

        var userIdString = GetClaimsValue(JwtRegisteredClaimNames.Sub);
        Guid.TryParse(userIdString, out var userId);

        var userName = GetClaimsValue(JwtRegisteredClaimNames.Name);
        var avatarUrl = GetClaimsValue(JwtRegisteredClaimNames.Picture);

        var roles = GetClaimsValues(ClaimTypes.Role)
            .Concat(GetClaimsValues("roles"))
            .SelectMany(r => r.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(r => r.Trim())
            .Distinct()
            .ToList();

        return new ClientContext(
            userId,
            ipAddress,
            deviceId,
            userName,
            email,
            phoneNumber,
            avatarUrl,
            roles);
    }

    private IEnumerable<string> GetClaimsValues(string claimType)
    {
        return accessor.HttpContext?.User.Claims
            .Where(c => c.Type == claimType)
            .Select(c => c.Value) ?? [];
    }

    private string GetClaimsValue(string claimType)
    {
        var value = accessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == claimType)?.Value;

        return value ?? string.Empty;
    }

    private static Guid? TryParseNullableGuid(string value)
    {
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}