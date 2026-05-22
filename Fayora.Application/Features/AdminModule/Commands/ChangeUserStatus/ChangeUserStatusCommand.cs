using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.IdentityModule;
using System;

namespace Fayora.Application.Features.AdminModule.Commands.ChangeUserStatus;

public record ChangeUserStatusCommand(Guid UserId, UserStatus Status) : ICommand<Result<Success>>;
