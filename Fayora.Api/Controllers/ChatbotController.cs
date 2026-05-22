using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Application.Features.ChatbotModule.Commands.SendChatbotMessage;
using Fayora.Application.Features.ChatbotModule.Queries.GetChatbotHistory;
using Fayora.Contracts.ChatbotModule;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatbotController(ISender sender, IClientContextProvider clientContextProvider) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> SendMessageAsync(
        [FromHeader(Name = AppHeaders.DeviceId)] string deviceId,
        [FromBody] SendChatbotMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest("Device ID header is required.");
        }

        var context = clientContextProvider.GetContext();
        Guid? userId = context.UserId == Guid.Empty ? null : context.UserId;

        var command = new SendChatbotMessageCommand(
            deviceId,
            request.Content,
            request.SessionId,
            userId);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            value =>
            {
                object? responseObj = null;
                try
                {
                    responseObj = JsonSerializer.Deserialize<object>(value.ResponseJson);
                }
                catch
                {
                    // Fallback to text if the response cannot be parsed as JSON
                    responseObj = new { text = value.ResponseJson };
                }

                return Ok(new
                {
                    SessionId = value.SessionId,
                    Response = responseObj
                });
            },
            Problem
        );
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryAsync(
        [FromHeader(Name = AppHeaders.DeviceId)] string deviceId,
        [FromQuery] Guid? sessionId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest("Device ID header is required.");
        }

        var query = new GetChatbotHistoryQuery(deviceId, sessionId);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            value => Ok(value),
            Problem
        );
    }
}
