using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateUser;

public class UpdateUserCommandHandler(IAdminRepository adminRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await adminRepository.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Error.NotFound($"User with ID {request.UserId} not found.");

        user.AdminUpdateDetails(request.FirstName, request.LastName, request.Email, request.PhoneNumber);
        user.AdminUpdateStatus(request.Status);
        user.AdminUpdateRoles(request.Roles);

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
