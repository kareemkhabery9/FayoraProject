using System;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.NotificationModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.NotificationModule;

namespace Fayora.Application.Features.TouristModule.Commands.RegisterDeviceToken;

public class RegisterDeviceTokenCommandHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    IClientContextProvider clientContextProvider) : ICommandHandler<RegisterDeviceTokenCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var clientContext = clientContextProvider.GetContext();
        Guid? userId = (clientContext != null && clientContext.UserId != Guid.Empty) ? clientContext.UserId : null;

        var existingToken = await notificationRepository.GetDeviceTokenByTokenAsync(request.Token, cancellationToken);

        if (existingToken != null)
        {
            existingToken.UpdateActivity(userId, request.DeviceType);
        }
        else
        {
            var deviceToken = DeviceToken.Create(userId, request.Token, request.DeviceType);
            await notificationRepository.AddDeviceTokenAsync(deviceToken, cancellationToken);
        }

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
