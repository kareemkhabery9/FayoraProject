using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandHandler(IAdminRepository adminRepository, IUnitOfWork unitOfWork) : ICommandHandler<ChangeUserStatusCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await adminRepository.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Error.NotFound($"User with ID {request.UserId} not found.");

        user.AdminUpdateStatus(request.Status);
        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
