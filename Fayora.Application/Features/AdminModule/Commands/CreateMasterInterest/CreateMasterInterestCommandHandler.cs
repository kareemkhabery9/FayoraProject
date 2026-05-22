using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.TouristModule;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.CreateMasterInterest;

public class CreateMasterInterestCommandHandler(
    IAdminRepository adminRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateMasterInterestCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateMasterInterestCommand request, CancellationToken cancellationToken)
    {
        var interest = new MasterInterest(request.Code, request.Name, request.IconUrl, request.SortOrder);

        adminRepository.AddMasterInterest(interest);
        await unitOfWork.CommitChangesAsync(cancellationToken);

        return interest.Id;
    }
}
