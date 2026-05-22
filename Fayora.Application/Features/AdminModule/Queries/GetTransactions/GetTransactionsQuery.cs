using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.FinancialTransactions;
using Fayora.Domain.Common.Results;
using System;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetTransactions;

public record GetTransactionsQuery(
    int PageNumber,
    int PageSize,
    string? StatusFilter,
    DateTime? FromDate,
    DateTime? ToDate) : IQuery<Result<List<GetTransactionsResponse>>>;
