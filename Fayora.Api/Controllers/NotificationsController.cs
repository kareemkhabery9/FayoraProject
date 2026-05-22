using Fayora.Application.Features.TouristModule.Commands.RegisterDeviceToken;
using Fayora.Contracts.AdminModule.Notifications;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(ISender sender) : ApiController
{
    [HttpPost("register-token")]
    public async Task<IActionResult> RegisterDeviceToken(
        [FromBody] RegisterDeviceTokenRequest request,
        CancellationToken ct)
    {
        var command = new RegisterDeviceTokenCommand(request.Token, request.DeviceType);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }
}
