using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.UpdateAccommodation;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetAccommodations;

public class GetAccommodationsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetAccommodationsQuery, Result<List<GetAccommodationsResponse>>>
{
    public async Task<Result<List<GetAccommodationsResponse>>> Handle(GetAccommodationsQuery request, CancellationToken cancellationToken)
    {
        var accommodations = await adminRepository.GetAccommodationsAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.StatusFilter,
            cancellationToken);

        return accommodations;
    }
}
