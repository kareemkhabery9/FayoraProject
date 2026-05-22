using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Domain.Common.Interfaces.IdentityModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.IdentityModule;
using Fayora.Domain.Errors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.CreateUser;

public class CreateUserCommandHandler(
    IAdminRepository adminRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await adminRepository.IsEmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
            return UserErrors.DuplicateEmail;

        var userResult = User.CreateWithEmail(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            passwordHasher
        );

        if (userResult.IsError)
            return userResult.Errors;

        var user = userResult.Value;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneResult = user.ChangePhoneNumber(request.PhoneNumber);
            if (phoneResult.IsError)
                return phoneResult.Errors;
            user.VerifyPhone();
        }

        user.AdminUpdateRoles(request.Roles);
        user.VerifyEmail();

        adminRepository.AddUser(user);
        await unitOfWork.CommitChangesAsync(cancellationToken);

        return user.Id;
    }
}
