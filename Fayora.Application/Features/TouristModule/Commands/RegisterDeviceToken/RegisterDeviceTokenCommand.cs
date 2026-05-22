using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.TouristModule.Commands.RegisterDeviceToken;

public record RegisterDeviceTokenCommand(
    string Token,
    string DeviceType) : ICommand<Result<Success>>;
