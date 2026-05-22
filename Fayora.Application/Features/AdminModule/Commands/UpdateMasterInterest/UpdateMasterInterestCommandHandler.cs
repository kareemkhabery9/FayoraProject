using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateMasterInterest;

public class UpdateMasterInterestCommandHandler(
    IAdminRepository adminRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateMasterInterestCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateMasterInterestCommand request, CancellationToken cancellationToken)
    {
        var interest = await adminRepository.GetMasterInterestByIdAsync(request.Id, cancellationToken);
        if (interest is null)
            return Error.NotFound("MasterInterest.NotFound", "Master interest not found.");

        interest.UpdateDetails(request.Name, request.IconUrl, request.SortOrder);
        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
