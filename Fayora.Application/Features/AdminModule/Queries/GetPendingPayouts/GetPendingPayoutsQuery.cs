using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.FinancialTransactions;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetPendingPayouts;

public record GetPendingPayoutsQuery() : IQuery<Result<List<GetPendingPayoutsResponse>>>;
