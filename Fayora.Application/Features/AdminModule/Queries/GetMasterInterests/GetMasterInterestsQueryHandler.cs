using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.MasterInterests;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetMasterInterests;

public class GetMasterInterestsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetMasterInterestsQuery, Result<List<GetMasterInterestsResponse>>>
{
    public async Task<Result<List<GetMasterInterestsResponse>>> Handle(GetMasterInterestsQuery request, CancellationToken cancellationToken)
    {
        var interests = await adminRepository.GetMasterInterestsAsync(cancellationToken);
        return interests;
    }
}
