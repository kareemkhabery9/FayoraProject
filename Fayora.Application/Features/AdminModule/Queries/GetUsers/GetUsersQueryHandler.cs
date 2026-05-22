using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.GetUsers;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetUsers;

public class GetUsersQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetUsersQuery, Result<List<GetUsersResponse>>>
{
    public async Task<Result<List<GetUsersResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await adminRepository.GetUsersAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.RoleFilter,
            request.StatusFilter,
            cancellationToken);

        return users;
    }
}
