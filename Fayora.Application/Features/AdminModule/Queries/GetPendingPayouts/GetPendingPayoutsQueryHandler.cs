using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.FinancialTransactions;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetPendingPayouts;

public class GetPendingPayoutsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetPendingPayoutsQuery, Result<List<GetPendingPayoutsResponse>>>
{
    public async Task<Result<List<GetPendingPayoutsResponse>>> Handle(GetPendingPayoutsQuery request, CancellationToken cancellationToken)
    {
        var payouts = await adminRepository.GetPendingPayoutsAsync(cancellationToken);
        return payouts;
    }
}
