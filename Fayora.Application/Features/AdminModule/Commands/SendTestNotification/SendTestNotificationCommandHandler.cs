using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Services.SharedModule;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Commands.SendTestNotification;

public class SendTestNotificationCommandHandler(IFirebaseNotificationService firebaseNotificationService)
    : ICommandHandler<SendTestNotificationCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(SendTestNotificationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Error.Validation("Notification.InvalidToken", "Token cannot be null or empty.");
        }

        var success = await firebaseNotificationService.SendToTokenAsync(
            request.Token,
            "Fayora Test Notification",
            "This is a test notification from the Fayora FCM Campaign Manager.",
            imageUrl: null,
            cancellationToken);

        if (!success)
        {
            return Error.Failure("Notification.SendFailed", "Failed to deliver the test push notification. Check server logs for details.");
        }

        return Result.Success;
    }
}
