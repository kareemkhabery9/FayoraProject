using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Commands.SendTestNotification;

public record SendTestNotificationCommand(string Token) : ICommand<Result<Success>>;
