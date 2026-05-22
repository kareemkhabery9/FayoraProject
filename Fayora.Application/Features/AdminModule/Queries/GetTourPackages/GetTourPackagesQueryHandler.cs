using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.UpdateTourPackage;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetTourPackages;

public class GetTourPackagesQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetTourPackagesQuery, Result<List<GetTourPackagesResponse>>>
{
    public async Task<Result<List<GetTourPackagesResponse>>> Handle(GetTourPackagesQuery request, CancellationToken cancellationToken)
    {
        var packages = await adminRepository.GetTourPackagesAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.StatusFilter,
            cancellationToken);

        return packages;
    }
}
