using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.IdentityModule;
using System;

namespace Fayora.Application.Features.AdminModule.Commands.CreateUser;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    Role Roles) : ICommand<Result<Guid>>;
