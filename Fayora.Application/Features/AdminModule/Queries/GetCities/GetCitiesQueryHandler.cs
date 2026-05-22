using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.Cities;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetCities;

public class GetCitiesQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetCitiesQuery, Result<List<GetCitiesResponse>>>
{
    public async Task<Result<List<GetCitiesResponse>>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        var cities = await adminRepository.GetCitiesAsync(cancellationToken);
        return cities;
    }
}
