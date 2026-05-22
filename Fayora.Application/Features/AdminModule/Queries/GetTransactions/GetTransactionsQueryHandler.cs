using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.FinancialTransactions;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetTransactions;

public class GetTransactionsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetTransactionsQuery, Result<List<GetTransactionsResponse>>>
{
    public async Task<Result<List<GetTransactionsResponse>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await adminRepository.GetTransactionsAsync(
            request.PageNumber, request.PageSize, request.StatusFilter, request.FromDate, request.ToDate, cancellationToken);
        return transactions;
    }
}
