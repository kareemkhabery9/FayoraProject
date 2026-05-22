using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.GetBookings;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetBookings;

public record GetBookingsQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    string? StatusFilter,
    string? PaymentFilter,
    string? ServiceTypeFilter) : IQuery<Result<List<GetBookingsResponse>>>;
