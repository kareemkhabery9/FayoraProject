using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.ToggleMasterInterest;

public class ToggleMasterInterestCommandHandler(
    IAdminRepository adminRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ToggleMasterInterestCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ToggleMasterInterestCommand request, CancellationToken cancellationToken)
    {
        var interest = await adminRepository.GetMasterInterestByIdAsync(request.Id, cancellationToken);
        if (interest is null)
            return Error.NotFound("MasterInterest.NotFound", "Master interest not found.");

        interest.IsActive = !interest.IsActive;
        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
