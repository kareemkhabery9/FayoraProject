using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.UpdateLocation;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetLocations;

public class GetLocationsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetLocationsQuery, Result<List<GetLocationsResponse>>>
{
    public async Task<Result<List<GetLocationsResponse>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var locations = await adminRepository.GetLocationsAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            cancellationToken);

        return locations;
    }
}
