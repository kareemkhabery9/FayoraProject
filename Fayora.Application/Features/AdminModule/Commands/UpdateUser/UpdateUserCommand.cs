using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.IdentityModule;
using System;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    UserStatus Status,
    Role Roles) : ICommand<Result<Success>>;
